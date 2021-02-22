test: 
	g++ test.cpp -lev -o test.out
	./test.out

example: functions
	g++ functions.o example.cpp -lev -o example.out
	./example.out
functions:
	g++ -c functions.cpp -o functions.o
clean: 
	rm -r *.o *.a *.out -f
	clear
