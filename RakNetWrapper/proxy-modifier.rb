require 'tempfile'
require 'optparse'
opt = OptionParser.new

OPTS = {}

opt.on('-s', 'Sets SuppressUnmanagedCodeSecurity in an attribute of a proxy class.') do |v| OPTS[:s] = v end
opt.on('-r', 'Adds MakeByRefType to a code of a Director class.') do |v| OPTS[:r] = v end

opt.parse!(ARGV)

srcname = ARGV.shift
temp = Tempfile::new('proxy')

if OPTS[:s]
  open(srcname) do |f|
    f.each do |line|
      if line[/\[(DllImport)\(.*\)\]/]
        temp.puts line[0, line.index('[')] << '[System.Security.SuppressUnmanagedCodeSecurity()]'
      end
      temp.puts line
    end
  end
elsif OPTS[:r]
  f = open(srcname)
  presence_table = []
  srclines = f.readlines
  # gather 'ref' information
  srclines.each do |line|
    if m = /(?:public delegate)\s+\w+\s+(?:\w+)\((.*)\)/.match(line)
      presence_of_ref = {}
      pos = 0
      parms = m[1].split(',')
      parms.each do |parm|
        parm.strip!
        if parm[/^(ref|out)\s/]
          presence_of_ref[pos] = true
        end
        pos = pos + 1
      end
      presence_table << presence_of_ref      
    end
  end
  
  # add MakeByRefType
  match_line_idx = 0
  srclines.each do |line|
    pattern = /(?:private static Type\[\]).*(?:new Type\[\])\s+\{(.*)\}/
    if m = pattern.match(line)
      method_types = m[1].split(',')
      method_types.map! do |type|
        type.strip
      end
      presence_table[match_line_idx].each_pair do |key, value|
        method_types[key] << '.MakeByRefType()'
      end
      line[pattern,1] = method_types.join(', ')
      match_line_idx = match_line_idx + 1
    end
    temp.puts line
  end 
  f.close
else
  abort 'You must pass options string.'
end

temp.close
temp.open

open(srcname, "w") do |f| 
  temp.each do |line| 
    f.puts(line) 
  end 
end

temp.close(true)
