 #define false 0
#define true 1
#include <stdio.h> 
#include <time.h>
#include <pthread.h>
#include "list.h"

int res[2][2] = { {0,0},{0,0} };
pthread_mutex_t mutex;
typedef struct _Args
{
	int bit;
	List* list;
} Args;

int countBits(int bit, int number)
{
	unsigned int num = number;
	int count = 0;
	do
	{
		if ((num & 1) == bit)
			count++;
		num >>= 1;
	} while (num != 0);
	//printf("%d %d\n",number,count);
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
				removeelem(((arg->bit) ? arg->list->size - 1 : 0),arg->list);
			pthread_mutex_unlock(&mutex);
			start = (arg->bit) ? arg->list->end : arg->list->start;
			i++;
	}
	//pthread_mutex_lock(&mutex);
		res[arg->bit][0] = count; res[arg->bit][1] = i;
	//pthread_mutex_unlock(&mutex);
	return 0;
}

int main(void)
{
	int print_list = false;
	srand(time(NULL));
	List *list = (List*)malloc(sizeof(List));		list = createlist();
	int n = 10000;
	for (int i = 0; i < n; i++)
	{
		addelem(rand()%20-10+1,0,list);
	}
	if (print_list)
	{
		print(list);
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
