#include <iostream>
#include <stdio.h>
using namespace std;


class Node
{
public:
	int number;
	class Node* prev;
	class Node* next;
};

class List
{
public:
	int size;
	class Node* start;
	class Node* end;

	Node* find(int pos)
	{
		class Node* tmp = this->start;
		int i = 0;
		while (tmp != nullptr && i++ < pos) tmp = tmp->next;
		return tmp;
	}

	void add(int number, int pos = 0)
	{
		class Node* tmp = nullptr;
		class Node* newnode = (Node*)malloc(sizeof(Node));
		if (this->size > 0)
		{
			tmp = this->find(pos);
			if (tmp != nullptr)
			{
				newnode->number = number;
				newnode->next = tmp;
				newnode->prev = tmp->prev;
				tmp->prev = newnode;
				if (newnode->prev != nullptr)
				{
					newnode->prev->next = newnode;
				}
				else
				{
					this->start = newnode;
				}
			}
			else
			{
				cout << "incorrect position\n"; return;
			}
		}
		else
		{
			newnode->number = number;
			newnode->next = newnode->prev = nullptr;
			this->start = newnode; this->end = newnode;
		}
		size++;
	}

	void remove(int pos)
	{
		if (pos < 0 || pos > size)
		{
			cout << "no element\n"; return;
		}
		int i = 0;
		class Node* tmp = (Node*) malloc(sizeof(Node));
		tmp = this->start;
		while (i < pos)
		{
			i++;
			tmp = tmp->next;
		}
		if (tmp->prev!=nullptr)
		{
			tmp->prev->next=tmp->next;
		}
		if (tmp->next!=nullptr)
		{
			tmp->next->prev=tmp->prev;
		}
    	if (tmp->prev==nullptr) {
    	    this->start = tmp->next;
    	}
    	if (tmp->next==nullptr) {
    	    this->end = tmp->prev;
    	}
    	free(tmp);
		size--;
	}

	void print()
	{
		class Node* tmp = this->start;
		for (int i = 0; i < this->size; i++)
		{
			cout << tmp->number << ' ';
			tmp = tmp->next;
		}
		cout << endl;
	}

	List* createlist() {
		List* tmp = (List*)malloc(sizeof(List));
		tmp->size = 0;
		tmp->start = tmp->end = nullptr;
		return tmp;
	}

	void deletelist(List** list) {
		Node* tmp = (*list)->start;
		Node* next = nullptr;
		while (tmp) {
			next = tmp->next;
			free(tmp);
			tmp = next;
		}
		free(*list);
		(*list) = nullptr;
	}
};
