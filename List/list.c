#include <stdio.h>
#include <stdlib.h>
#include "list.h"

Node* find(int pos, List *list)
{
	Node* tmp = list->start;
	int i = 0;
	while (tmp != NULL && i++ < pos) tmp = tmp->next;
	return tmp;
}

void addelem(int number, int pos, List *list)
{
	Node* tmp = NULL;
	Node* newnode = (Node*)malloc(sizeof(Node));
	if (list->size > 0)
	{
		tmp = find(pos, list);
		if (tmp != NULL)
		{
			newnode->number = number;
			newnode->next = tmp;
			newnode->prev = tmp->prev;
			tmp->prev = newnode;
			if (newnode->prev != NULL)
			{
				newnode->prev->next = newnode;
			}
			else
			{
				list->start = newnode;
			}
		}
		else
		{
			printf("incorrect position\n"); return;
		}
	}
	else
	{
		newnode->number = number;
		newnode->next = newnode->prev = NULL;
		list->start = newnode; list->end = newnode;
	}
	list->size++;
}
void removeelem(int pos,List *list)
{
	if (pos < 0 || pos >= list->size)
	{
		printf("no element\n"); return;
	}
	int i = 0;
	Node* tmp = (Node*) malloc(sizeof(Node));
	tmp = list->start;
	while (i < pos)
	{
		i++;
		tmp = tmp->next;
	}
	if (tmp->prev!=NULL)
	{
		tmp->prev->next=tmp->next;
	}
	if (tmp->next!=NULL)
	{
		tmp->next->prev=tmp->prev;
	}
	if (tmp->prev==NULL) {
		list->start = tmp->next;
	}
	list->size--;
}
void print(List *list)
{
	Node* tmp = list->start;
	for (int i = 0; i < list->size; i++)
	{
		printf("%d ",tmp->number);
		if ((i+1)%15==0)
		printf("\n");
		tmp = (tmp->next)?tmp->next:tmp;
	}
	printf("\n");
}
List* createlist()
{
	List* tmp = (List*)malloc(sizeof(List));
	tmp->size = 0;
	tmp->start = tmp->end = NULL;
	return tmp;
}
void deletelist(List** list)
{
	Node* tmp = (*list)->start;
	Node* next = NULL;
	while (tmp) {
		next = tmp->next;
		free(tmp);
		tmp = next;
	}
	free(*list);
	(*list) = NULL;
}
