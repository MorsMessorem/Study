#include <iostream>
#include <stdio.h> 
#include <time.h>
#include <pthread.h>
#include "list.cpp"
using namespace std;

int res[2][2] = { {0,0},{0,0} };
pthread_mutex_t mutex;
class Args
{
public:
	int bit;
	List* list;
};
int countBits(int bit, int number)
{
	int count = 0;
	do
	{
		if ((number & 1) == bit)
			count++;
		number >>= 1;
	} while (number != 0);
	return count;
}

void* threadBits(void* args)
{
	Args* arg = (Args*)args;
	Node* start = (Node*)malloc(sizeof(Node));
	start = (arg->bit) ? arg->list->end : arg->list->start;
	int i = 0;
	int count = 0;
	while (arg->list->size>0)
	{
			pthread_mutex_lock(&mutex);
				if (arg->list->size==0)
					{
					pthread_mutex_unlock(&mutex);
					break;
					}
				count += countBits(arg->bit, start->number);
				arg->list->remove((arg->bit) ? arg->list->size - 1 : 0);
			pthread_mutex_unlock(&mutex);
			start = (arg->bit) ? arg->list->end : arg->list->start;
			i++;
	}
	//pthread_mutex_lock(&mutex);
		res[arg->bit][0] = count; res[arg->bit][1] = i;
	//pthread_mutex_unlock(&mutex);
	return 0;
}

int main()
{
	bool print_list = false;
	srand(time(NULL));
	class List *list = (List*)malloc(sizeof(List));		list = list->createlist();
	int n = 10000;
	for (int i = 0; i < n; i++)
	{
		list->add(2);
	}
	if (print_list)
	{
		list->print();
	}
	pthread_t threads[2];
	Args args[2];
	pthread_mutex_init(&mutex, NULL);
	for (int i = 0; i < 2; i++)
	{

		args[i].list = list;
		args[i].bit = i;
		if (pthread_create(&threads[i], NULL, threadBits, (void*)&args[i])) {
			printf("Error: pthread_create failed!\n");
			return 1;
		}
	}
	int status_addr;
	for (int i = 0; i < 2; i++)
	{
		if (pthread_join(threads[i], (void**)&status_addr)) {
			printf("Error: pthread_join failed!\n");
			return 1;
		}
	}
	printf("list length:\t\t\t\t%d\namount of zeros in list:\t\t%d\namount of counted elements:\t\t%d\namount of ones in list:\t\t\t%d\namount of counted elements:\t\t%d\nsummary amount of counted elements:\t%d\n", list->size, res[0][0], res[0][1], res[1][0], res[1][1], res[0][1] + res[1][1]);
	return 0;
}
