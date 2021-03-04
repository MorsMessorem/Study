#include <stdio.h>
#include <stdlib.h>

typedef struct _Node
{
	int number;
	struct _Node* prev;
	struct _Node* next;
} Node;

typedef struct _List
{
	int size;
	Node* start;
	Node* end;
} List;

Node* find(int pos, List *list);
void addelem(int number, int pos, List *list);
void removeelem(int pos, List *list);
void print();
List* createlist();
void deletelist(List **list);
