#include <ev.h>
#include <iostream>
#include <stdio.h> 
#include <string>
#include <vector>
#include <sys/types.h>
#include <sys/wait.h>
#include <unistd.h>
#include <math.h>
using namespace std;

bool finish_flag=false;
int num_count=2;
//length of answer
size_t len=1;
//pipe
int file_pipe[2];
pid_t pid;

string sum(string a, string b, int step, int B);
void multiply(const vector<int>& a, const vector<int>& b, vector<int>& res);
double** matrix(double** A, int n);

//watchers
ev_timer timeout_watcher;
ev_child cw;

//watcher's functions
static void child_cb (EV_P_ ev_child *w, int revents)
{
	ev_child_stop (EV_A_ w);
	//printf ("process %d exited with status %x\n", w->rpid, w->rstatus);
	if (!finish_flag)
	{
	printf("child done faster\n");
	finish_flag=true;
	}
	printf("result of child's calculations: ");
	//get answer
	char buf[256];
      	read(file_pipe[0],buf,len);
      	string received(buf);
      	cout<<received<<endl;      	
}

static void timeout_cb (EV_P_ ev_timer *w, int revents)
{
	cout <<"timeout\n";
	// this causes the innermost ev_run to stop iterating
	kill(pid,SIGKILL);
	ev_break (EV_A_ EVBREAK_ONE);	
}

int main (void)
{
int timeout_sec = 7;
string* num = new string[num_count];
num[0]="1234567890123456789012";
num[1]="9876543210987654321098";
len = num[0].length();
for (int i=1;i<num_count;i++)
	if (num[i].length()>len)
		len=num[i].length();
len*=num_count;

struct ev_loop *loop = EV_DEFAULT;

if (pipe(file_pipe)<0)
    {exit(0);}
    
ev_timer_init (&timeout_watcher, timeout_cb, timeout_sec, 0.);
ev_timer_start (loop, &timeout_watcher);

pid = fork();

if (pid < 0)
{
	// error
	cout<<"error\n";
	exit(1);
}
else if (pid == 0)
{
	//cout << num[0] << endl << num[1] << endl;
	int n = 0;  //max int count after split
	int i = 1e7;
	int b = 4;
	int k = 3;
	int B; //split length
	// | Split
	for (int j = 0; j < num_count; j++)
	{
		if (num[j].length() / b < i)
			i = num[j].length() / b;
	}
	i = (i / k + 1);
	B = b * i;
	for (int j = 0; j < num_count; j++)
	{
		if (n < num[j].length() / B + 1)
			n = num[j].length() / B + 1;
	}
	int* inum = new int[n];
	int64_t* p;
	int64_t* q;
	int d=0;
	// | Split
	for (int l=0;l<num_count;l++)
	{
		i = 0;
		inum[0] = 0;
		//parse longint
		for (int k = 0; k < num[l].length(); k++)
		{
			inum[i] = inum[i] * 10 + (int)(num[l][k] - 48);
			if ((num[l].length() - k - 1) % B == 0)
			{
				i++;
				inum[i] = 0;
			}
		}
	
	d = n + n - 1;
	k = 0;
	if (l==0)
	p = new int64_t[d];
	q = new int64_t[d];
	//prepare for calculations | Assessment
	if (l==0)
	{
		for (int i = 0; i < d - 1; i++)
		{
			p[i] = 0;
			for (int j = 0; j < n; j++)
			{
				p[i] += inum[n - j - 1] * pow(k, j);
			}
			k = (i % 2 == 0) ? 0 - k - 1 : 0 - k;
		}
		p[d - 1] = inum[0];
		}
		else
		{
		for (int i = 0; i < d - 1; i++)
		{
			q[i] = 0;
			for (int j = 0; j < n; j++)
			{
				q[i] += inum[n - j - 1] * pow(k, j);
			}
			k = (i % 2 == 0) ? 0 - k - 1 : 0 - k;
		}
		q[d - 1] = inum[0];
		}
		// | Point multiplication
		if (l>0)
			for (int i = 0; i < d; i++)
			{
				p[i] = p[i] * q[i];
			}
	}
	double** r = new double* [d];
	k = 0;
	for (int i = 0; i < d - 1; i++)
	{
		r[i] = new double[d];
		for (int j = 0; j < d; j++)
		{
			r[i][j] = pow(k, j);
		}
		k = (i % 2 == 0) ? 0 - k - 1 : 0 - k;
	}
	r[d - 1] = new double[d];
	for (int i = 0; i < d - 1; i++)
		r[d - 1][i] = 0;
	r[d - 1][d - 1] = 1;
	// | Interpolation
	r = matrix(r, d);
	cout << endl;
	// | Rearrangement
	int64_t* res = new int64_t[d];
	for (int i = 0; i < d; i++)
	{
		res[i] = 0;
		for (int j = 0; j < d; j++)
		{
			res[i] += (int64_t)(r[i][j] * p[j]);
		}
	}
	string* sres = new string[d];
	for (int i = 0; i < d; i++)
		sres[i] = to_string(res[i]);
	//summ all
	for (int i = 0; i < d - 1; i++)
	{
		sres[i + 1] = sum(sres[i], sres[i + 1], i + 1, B);
	}
	cout << endl;
	//cout <<"res: " << sres[d - 1] << endl;
	char* char_arr;
    	char_arr = &sres[d-1][0];
	write(file_pipe[1],char_arr,strlen(char_arr));
	exit(1);
}
else
{
	//wait till child end?
	//0-execute when child stop; 1-stop or continued
	ev_child_init (&cw, child_cb, pid, 0);
	ev_child_start (EV_DEFAULT_ &cw);
	int pow_count=0;
	int n = 0;
	for (int j = 0; j < num_count; j++)
	{
		if (n < num[j].length())
			n = num[j].length();
	}
	pow_count = (n - 1) * num_count + 1;
	vector<int> a(n);
	vector<int> b(n);
	int* inum = new int[n];
	for (int l=0;l<num_count;l++)
	{
		for (int i = 0; i < num_count; i++)
		{
			for (int j = 0; j < n; j++)
			{
				if ((int)(num[l].length() - 1 - j) >= 0)
					inum[j] = num[l][num[l].length() - 1 - j] - 48;
				else
					inum[j] = 0;
			}
		}
		if (l == 0)
		{
			for (int j = 0; j < n; j++)
			{
				a[j] = inum[j];
			}
		}
		else
		{
			for (int j = 0; j < n; j++)
			{
				b[j] = inum[j];
			}
			multiply(a, b, a);
		}
	}
	int* res = new int[pow_count];
	for (int i = 0; i < pow_count; i++)
		{
		if (i < a.size())
			res[i] = a[i];
		else
			res[i] = 0;
	}
	for (int i = 0; i < pow_count - 1; i++)
	{
		if (res[i] / 10 > 0)
		{
			res[i + 1] += res[i] / 10;
			res[i] %= 10;
		}
	}
	string result = "";
	for (int i = 0; i < pow_count; i++)
	result = to_string(res[i]) + result;
	if (!finish_flag)
	{
		printf("parent done faster\n");
		finish_flag=true;
	}	
	cout << "result of parent's calculations: "<< result<<endl;	
}
ev_timer_stop(loop,&timeout_watcher);
ev_run (loop, 0);
return 0;
}
