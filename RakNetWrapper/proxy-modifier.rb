require 'tempfile'
srcname = ARGV.shift
temp = Tempfile::new('proxy')

open(srcname) do |f|
  f.each do |line|
    if line[/\[(DllImport)\(.*\)\]/]
      temp.puts line[0, line.index('[')] << '[System.Security.SuppressUnmanagedCodeSecurity()]'
    end
    temp.puts line
  end
end

temp.close
temp.open

open(srcname, "w") do |f| 
  temp.each do |line| 
    f.puts(line) 
  end 
end

temp.close(true)
