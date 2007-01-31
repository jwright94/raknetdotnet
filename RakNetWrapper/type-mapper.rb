module TypeHelper
  INT_TYPES = ['bool', 'signed char', 'unsigned char', 'short', 'unsigned short', 'int', 'unsigned int', 'long', 'unsigned long']
  FP_TYPES = ['float', 'double']
  CTYPES = INT_TYPES + FP_TYPES
  
  def ref(ctype)
    "#{ctype}&"
  end
  
  module_function :ref
end

# Rule definition helpers.
module DirectiveHelper
  include TypeHelper
  
  def member_template_specialize_in(ctypes, names)
    out = ''
    for name in names
      for ctype in ctypes
        out << "%template(#{name}) #{name}<#{ctype}>;\n"
      end
    end
    out
  end
  
  module_function :member_template_specialize_in
end

class TypeMapper
  include TypeHelper

  def initialize
    @scope_stack = []
  end
    
  def typemap_input_blittable(ctype, name, cstype)
    ctype_and_name = concat ctype, name
    <<EOS
%typemap(ctype)  #{ctype_and_name} "#{ctype}"
%typemap(imtype) #{ctype_and_name} "#{cstype}"
%typemap(cstype) #{ctype_and_name} "#{cstype}"
%typemap(in)     #{ctype_and_name} %{$1 = ($1_ltype)$input;%}
%typemap(csin)   #{ctype_and_name} "$csinput"
EOS
  end
  
  def typemap_inout_blittable(ctype, name, cstype)
    ctype_and_name = concat ctype, name
    <<EOS
%typemap(ctype)  #{ctype_and_name} "#{ctype}"
%typemap(imtype) #{ctype_and_name} "[In, Out] #{cstype}"
%typemap(cstype) #{ctype_and_name} "#{cstype}"
%typemap(in)     #{ctype_and_name} %{$1 = ($1_ltype)$input;%}
%typemap(csin)   #{ctype_and_name} "$csinput"
EOS
  end

  def typemap_ref_blittable(ctype, name, cstype)
    ctype_and_name = concat ctype, name
    <<EOS
%typemap(ctype)  #{ctype_and_name} "#{ctype}"
%typemap(imtype) #{ctype_and_name} "ref #{cstype}"
%typemap(cstype) #{ctype_and_name} "ref #{cstype}"
%typemap(in)     #{ctype_and_name} %{$1 = ($1_ltype)$input;%}
%typemap(csin)   #{ctype_and_name} "ref $csinput"
EOS
  end
  
  def typemap_void_ptr(ctype, name)
    ctype_and_name = concat ctype, name
    <<EOS
%typemap(ctype)                        #{ctype_and_name} "#{ctype}"
%typemap(imtype, out="IntPtr")         #{ctype_and_name} "IntPtr"
%typemap(cstype)                       #{ctype_and_name} "IntPtr"
%typemap(in)                           #{ctype_and_name} %{$1 = ($1_ltype)$input;%}
%typemap(out)                          #{ctype_and_name} %{$result = ($1_ltype)$1;%}
%typemap(csin)                         #{ctype_and_name} "$csinput"
%typemap(csout, excode=SWIGEXCODE)     #{ctype_and_name} {
    return $imcall;$excode
  }
%typemap(csdirectorin)                 #{ctype_and_name} "$iminput"
%typemap(csdirectorout)                #{ctype_and_name} "$cscall"
%typemap(csvarin, excode=SWIGEXCODE)   #{ctype_and_name} %{
    set {
      $imcall;$excode
    }
%}
%typemap(csvarout, excode=SWIGEXCODE)  #{ctype_and_name} %{
    get {
      return $imcall;$excode
    }
%}
EOS
  end
  
  def typemap_csvarout_blittable_array(ctype, name, element_cstype, length_code)
    ctype_and_name = concat ctype, name
    <<EOS
%typemap(ctype)                        #{ctype_and_name} "#{ctype}"
%typemap(imtype, out="IntPtr")         #{ctype_and_name} "#{element_cstype}[]"
%typemap(cstype)                       #{ctype_and_name} "#{element_cstype}[]"
%typemap(out)                          #{ctype_and_name} %{$result = ($1_ltype)$1;%} 
%typemap(csvarout, excode=SWIGEXCODE2) #{ctype_and_name} %{
    get {
      int length = (int)#{length_code};
      #{element_cstype}[] ret = new #{element_cstype}[length];
      IntPtr data = $imcall;
      System.Runtime.InteropServices.Marshal.Copy(data, ret, 0, length);$excode
      return ret;
    }
%}
%typemap(csvarin, excode=SWIGEXCODE2)  #{ctype_and_name} %{$excode%}
EOS
  end
  
  def start_scope(name=nil)
    state = {:name=>name, :clear_directives=>[]}
    @scope_stack.unshift(state)
    "/*** Begin #{name} ***/" if name
  end
  
  def apply(ctype, name, mapped_ctype, mapped_name='')
    current_clear_directives << "%clear #{mapped_ctype} #{mapped_name};"
    "%apply #{ctype} #{name} { #{mapped_ctype} #{mapped_name} };"
  end
  
  def apply_same_ctype(ctype, name, mapped_name='')
    apply(ctype, name, ctype, mapped_name)
  end
  
  def apply_to_creftypes(name, mapped_name='')
    out = ''
    for ctype in CTYPES
      out << apply_same_ctype(ref(ctype), name, mapped_name) << "\n"
    end
    out
  end
  
  def end_scope
    out = ''
    state = @scope_stack.shift
    state[:clear_directives].each {|directive|
      out << directive << "\n"
    }
    out << "/*** End #{state[:name]} ***/\n" if state[:name]
    out
  end
  
  private
  def current_clear_directives
    @scope_stack.first[:clear_directives]
  end
  
  def concat(ctype, name)
    ctype + ' ' + name
  end
end
