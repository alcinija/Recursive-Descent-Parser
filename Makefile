all: update check diagram

EXAMPLES=1 2 3 4 5 6 7

check: $(addprefix check-example, $(EXAMPLES))
	@echo "Your code has passed all checks"

output: $(addprefix example, $(addsuffix .output, $(EXAMPLES)))

lispish.exe: lispish.cs
	csc lispish.cs

diagram: diagram.png

diagram.png: state-diagram.gv
	dot state-diagram.gv -odiagram.png -Tpng

%.output: lispish.exe %.input
	@timeout -v 30s mono lispish.exe < $*.input  2>&1 > $*.output

check-%: %.output
	@diff -qb $*.output $*.expected
	@echo $*- Success!

update: 
	@echo "Pulling in any changes made to the assignment by the instructor"
	@git pull https://gitlab.csi.miamioh.edu/cse465/instructor/project2.git master

clean:
	rm -f *.output
	rm -f lispish.exe 
	rm -f lispish.pdb

submit: check
	git add -u
	git commit -m "Submitting"
	git push origin master
	echo "Your code has been submitted"
