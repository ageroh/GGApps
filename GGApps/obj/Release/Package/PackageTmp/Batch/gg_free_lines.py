import sqlite3, cStringIO, socket, sys
import codecs


def removeMatched_(string):
    string = re.sub(re.compile("[---------].*?\n" ) ,"" ,string) 				# remove all occurance singleline comments (//COMMENT\n ) from string
    return string



args = sys.argv

if  1 < len(args) < 4:
    file = args[1]
else:
    raise SystemExit('No Aguments.')
    
    

with open(file) as f:
    try:
        header = next(f)
    except StopIteration as e:
        print "File is empty"
     for line in f:
	print line 
    
    
# Open a file
#fo = open(file, "r+")
# print "Name of the file: ", fo.name

# line = fo.readline()
# print "Read Line: %s" % (line)

# line = fo.readline(5)
# print "Read Line: %s" % (line)

# # Close opend file
# fo.close()
    