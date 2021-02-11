#define _USE_MATH_DEFINES
#include <cmath>
#include <algorithm>
#include <stdio.h>
#include "mpi.h"
#include <iostream>
#include <time.h>
#include <math.h>
#include <string>
#include <vector>
#include <complex>

using namespace std;


int polinom_size = 3; // max polinom degree
int polinom_count = 2; //polinom count
int pow_count = polinom_count * (polinom_size - 1) + 1; //degree of final polinom
//mult polinoms
double* multpol(double* pol1, double* pol2)
{
	double* pol3 = new double[pow_count];
	for (int i = 0; i < pow_count; i++)
	{
		pol3[i] = 0;
	}
	for (int i = 0; i < pow_count; i++)
	{
		for (int j = 0; j < pow_count; j++)
		{
			if (i + j < pow_count)
				pol3[i + j] += pol1[i] * pol2[j];
		}
	}
	return pol3;
}
int lab4(int argc, char* argv[]);

int num_count = 2; // count of numbres
////find A^(-1) matrix
double** matrix(double** A, int n);
////column addition
string sum(string a, string b, int step, int B)
{
	string c = "";
	int i = 0;
	int j = 0;
	for (i = 0; i < step; i++)
		for (j = 0; j < B; j++)
			b = b + "0";
	i = a.length() - 1;
	j = b.length() - 1;
	int k = 0;
	while (true)
	{
		int r = k;
		if (i >= 0)
			r += a[i] - 48;
		if (j >= 0)
			r += b[j] - 48;
		c = to_string(r % 10) + c;
		k = r / 10;
		if ((i <= 0) && (j <= 0))
			break;
		i--; j--;
	}
	if (k == 1)
		c = to_string(k) + c;
	return c;
}
int lab5_1(int argc, char* argv[]);
////mult polinoms by fast fourier transform
void multiply(const vector<int>& a, const vector<int>& b, vector<int>& res);
int lab6(int argc, char* argv[]);


int main(int argc, char* argv[])
{
	//polinom multyply
	lab4(argc, argv);
	//longint multyply
	//lab5_1(argc, argv);
	//longint multyply
	//lab6(argc, argv);
}

int lab4(int argc, char* argv[])
{
	bool one = false;
	int ProcNum, ProcRank, RecvRank;
	double** coefficients = new double* [polinom_count];
	double* sendpol = new double[pow_count];
	double* result = new double[pow_count];
	double* tmp = new double[pow_count];
	MPI_Status Status;
	MPI_Init(&argc, &argv);
	MPI_Comm_size(MPI_COMM_WORLD, &ProcNum);
	MPI_Comm_rank(MPI_COMM_WORLD, &ProcRank);

	MPI_Datatype type; //new type
	MPI_Type_contiguous(pow_count, MPI_DOUBLE, &type);//construct type
	MPI_Type_commit(&type);//create type

	//initialization, print polinoms, send polinoms
	if (ProcRank == 0)
	{
		//initialize
		for (int i = 0; i < pow_count; i++)
		{
			sendpol[i] = 0;
			result[i] = 0;
		}
		for (int i = 0; i < polinom_count; i++)
		{
			coefficients[i] = new double[polinom_size];
			for (int j = polinom_size - 1; j >= 0; j--)
			{
				coefficients[i][j] = (double)i / 10 + j;
			}
		}
		//prin polinoms,send polinoms for calculations
		for (int i = 0; i < polinom_count; i++)
		{
			cout << "polinom " << i + 1 << ":\t";
			for (int j = polinom_size - 1; j >= 0; j--)
			{
				cout << coefficients[i][j] << "x^" << (j);
				if (j != 0)
					cout << "+";
				sendpol[j] = coefficients[i][j];
			}
			for (int j = pow_count - 1; j >= polinom_size; j--)
			{
				sendpol[j] = 0;
			}
			cout << endl;
			MPI_Send(sendpol, 1, type, i % (ProcNum - 1) + 1, 0, MPI_COMM_WORLD);
		}
	}
	else
	{
		int recvcount = polinom_count / (ProcNum - 1);//count of received messages
		if (polinom_count > ProcNum)
		{
			if (polinom_count % (ProcNum - 1) > ProcRank + 1)
				recvcount++;
		}
		else
		{
			if (polinom_count > ProcRank - 1)
				recvcount++;
		}
		for (int i = 0; i < recvcount; i++)
		{
			MPI_Recv(tmp, 1, type, 0, 0, MPI_COMM_WORLD, &Status);
			if (i > 0)
			{
				tmp = multpol(tmp, sendpol);
				for (int i = 0; i < pow_count; i++)
					sendpol[i] = tmp[i];
			}
			else
				for (int i = 0; i < pow_count; i++)
					sendpol[i] = tmp[i];
		}
	}

	MPI_Barrier(MPI_COMM_WORLD);
	if (ProcRank != 0)
	{
		//send calculated data
		if (ProcNum < polinom_count)
			MPI_Send(sendpol, 1, type, 0, 0, MPI_COMM_WORLD);
		else
			if (ProcRank <= polinom_count)
			{
				MPI_Send(sendpol, 1, type, 0, 0, MPI_COMM_WORLD);
			}
	}
	else
	{
		//data collection
		//final multyply
		for (int i = 1; i < min(ProcNum,polinom_count+1); i++)
		{
			MPI_Recv(tmp, 1, type, i, 0, MPI_COMM_WORLD, &Status);
			if (i > 1)
			{
				tmp = multpol(tmp, sendpol);
				for (int i = 0; i < pow_count; i++)
					sendpol[i] = tmp[i];
			}
			else
				for (int i = 0; i < pow_count; i++)
					sendpol[i] = tmp[i];
		}
		cout << "result " << ":\t";
		for (int i = pow_count - 1; i >= 0; i--)
		{
			result[i] = sendpol[i];
			cout << result[i] << "x^" << (i);
			if (i != 0)
				cout << "+";
		}
	}
	MPI_Type_free(&type);//delete type
	MPI_Finalize();
	return 0;
}

//full toom
int lab5_1(int argc, char* argv[])
{
	int ProcNum, ProcRank, RecvRank, CalcRank;
	string* num = new string[num_count];
	num[0] = "1234567890123456789012";
	num[1] = "987654321987654321098";

	//cout << num[0] << endl << num[1] << endl;
	int n = 0;  //max int count after split
	int i = INT_MAX;
	int b = 4;
	int k = 3;
	int B; //split length
	MPI_Status Status;
	MPI_Init(&argc, &argv);
	MPI_Comm_size(MPI_COMM_WORLD, &ProcNum);
	MPI_Comm_rank(MPI_COMM_WORLD, &ProcRank);
	if (ProcNum < 3)
	{
		cout << "This program for 3 processes";
		MPI_Finalize();
		return 0;
	}
	if (ProcRank == 0)
	{
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
	}
	MPI_Bcast(&n, 1, MPI_INT, 0, MPI_COMM_WORLD);
	MPI_Bcast(&B, 1, MPI_INT, 0, MPI_COMM_WORLD);
	MPI_Datatype longinttype;//new type
	MPI_Type_contiguous(n, MPI_INT, &longinttype);//create type
	MPI_Type_commit(&longinttype);//commit type

	MPI_Comm Calculations;
	MPI_Group CalculationGroup;
	MPI_Comm_group(MPI_COMM_WORLD, &CalculationGroup);
	MPI_Group_incl(CalculationGroup, 2, new int[] {1, 2}, &CalculationGroup);//new group (1->0,2->1 proc)
	MPI_Group_rank(CalculationGroup, &CalcRank);

	MPI_Comm_create(MPI_COMM_WORLD, CalculationGroup, &Calculations);//new comm (2,3 proc)
	int* inum = new int[n];
	if (ProcRank == 0)
	{
		// | Split
		for (int j = 0; j < num_count; j++)
		{
			i = 0;
			inum[0] = 0;
			//parse lonhint
			for (int k = 0; k < num[j].length(); k++)
			{
				inum[i] = inum[i] * 10 + (int)(num[j][k] - 48);
				if ((num[j].length() - k - 1) % B == 0)
				{
					i++;
					inum[i] = 0;
				}
			}
			//send to calcultion processes
			MPI_Send(inum, 1, longinttype, j % 2 + 1, 0, MPI_COMM_WORLD);
		}
	}
	else
	{
		int d = n + n - 1;
		MPI_Datatype longinttype2; //new type for result
		MPI_Type_contiguous(d, MPI_INT64_T, &longinttype2);
		MPI_Type_commit(&longinttype2);
		if (CalcRank == 1)
		{
			int64_t* p = new int64_t[d];
			MPI_Recv(inum, 1, longinttype, 0, 0, MPI_COMM_WORLD, &Status);
			int k = 0;
			//prepare for calculations | Assessment
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
			//send to main calc process
			MPI_Send(p, 1, longinttype2, 0, 0, Calculations);
		}
		else
		{
			int64_t* p = new int64_t[d];
			MPI_Recv(inum, 1, longinttype, 0, 0, MPI_COMM_WORLD, &Status);
			int k = 0;
			//prepara for calculations | Assessment
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
			int64_t* q = new int64_t[d];
			MPI_Recv(q, 1, longinttype2, 1, 0, Calculations, &Status);
			// | Point multiplication
			for (int i = 0; i < d; i++)
			{
				p[i] = p[i] * q[i];
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
			//round r
			for (int i = 0; i < d; i++)
			{
				for (int j = 0; j < d; j++)
				{
					if (abs(r[i][j]) < 1e-10)
						r[i][j] = 0;
				}
			}
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
			cout << sres[d - 1] << endl;
			//cout << "1219326312467611632493760095208585886175176" << endl;
		}
		MPI_Type_free(&longinttype2);
	}
	if (CalcRank >= 0)
	{
		MPI_Group_free(&CalculationGroup);
		MPI_Comm_free(&Calculations);
	}
	MPI_Type_free(&longinttype);
	MPI_Finalize();
	return 0;
}
int lab6(int argc, char* argv[])
{
	int n = 0;
	num_count = 4;
	int ProcNum, ProcRank, RecvRank, CalcRank;
	string* num = new string[num_count]
	{
		"157",
		"171",
		"157",
		"171"
	};
	int b = 1;
	MPI_Status Status;
	MPI_Init(&argc, &argv);
	MPI_Comm_size(MPI_COMM_WORLD, &ProcNum);
	MPI_Comm_rank(MPI_COMM_WORLD, &ProcRank);
	if (ProcNum != 4)
	{
		cout << "This program for 4 processes";
		MPI_Finalize();
		return 0;
	}
	if (ProcRank == 0)
	{
		for (int j = 0; j < num_count; j++)
		{
			if (n < num[j].length())
				n = num[j].length();
		}
		pow_count = (n - 1) * num_count + 1;
	}
	MPI_Bcast(&n, 1, MPI_INT, 0, MPI_COMM_WORLD);
	MPI_Bcast(&pow_count, 1, MPI_INT, 0, MPI_COMM_WORLD);

	MPI_Datatype polinom;
	MPI_Type_contiguous(n, MPI_INT, &polinom);//new type
	MPI_Type_commit(&polinom);

	MPI_Datatype polinom2;
	MPI_Type_contiguous(pow_count, MPI_INT, &polinom2);//new type
	MPI_Type_commit(&polinom2);

	MPI_Comm GridComm; //Cart commuicator
	int GridNum, GridRank;
	int dims[2], periods[2], reorder = 1;
	dims[0] = dims[1] = 2;
	periods[0] = periods[1] = 1;
	MPI_Cart_create(MPI_COMM_WORLD, 2, dims, periods, reorder, &GridComm);
	MPI_Comm RowComm, ColComm; //row/column communicators
	MPI_Comm_size(GridComm, &GridNum);
	MPI_Comm_rank(GridComm, &GridRank);
	int subdims[2];
	subdims[0] = 0; 
	subdims[1] = 1; 
	MPI_Cart_sub(GridComm, subdims, &RowComm);
	subdims[0] = 1;
	subdims[1] = 0;
	MPI_Cart_sub(GridComm, subdims, &ColComm);
	int coord[2];
	MPI_Cart_coords(GridComm, GridRank, dims[0], coord);
	int coordsum = coord[0] + coord[1];
	int RowRank, RowNum, ColNum, ColRank;
	MPI_Comm_size(RowComm, &RowNum);
	MPI_Comm_rank(RowComm, &RowRank);
	MPI_Comm_size(ColComm, &ColNum);
	MPI_Comm_rank(ColComm, &ColRank); 
	
	int* inum = new int[n];
	if (coordsum %2 == 0)
	{
		// (0,0) proc
		//parse nums 
		if (GridRank == 0)
		{
			for (int i = 0; i < num_count; i++)
			{
				for (int j = 0; j < n; j++)
				{
					if (num[i].length() - 1 - j >= 0)
						inum[j] = num[i][num[i].length() - 1 - j] - 48;
					else
						inum[j] = 0;
				}
				//send to neighbor nodes
				// (0,0) - > (0,1)
				//	 |
				//   v
				// (1,0)     (1,1)
				if (i % 2 == 0)
				{
					MPI_Send(inum, 1, polinom, RowRank + 1, 0, RowComm);
				}
				else
				{
					MPI_Send(inum, 1, polinom, ColRank + 1, 0, ColComm);
				}
			}
		}
		//(1,1) proc
		else
		{
			vector<int> a(pow_count);
			vector<int> b(pow_count);
			int* res = new int[pow_count];
			//receive results from calculate nodes
			//final miltiply
			for (int i = 1; i < GridNum-1; i++)
			{
				if (i % 2 == 1)
				{
					MPI_Recv(res, 1, polinom2, 0, 0, ColComm, &Status);
				}
				else
				{
					MPI_Recv(res, 1, polinom2, 0, 0, RowComm, &Status);
				}
				if (i == 1)
				{
					for (int j = 0; j < pow_count; j++)
					{
						a[j] = res[j];
					}
				}
				else
				{
					for (int j = 0; j < pow_count; j++)
					{
						b[j] = res[j];
					}
					multiply(a, b, a);
				}
			}
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
			cout << result;
		}
	}
	// (0,1),(1,0) proc
	else
	{
		vector<int> a(n);
		vector<int> b(n);
		int recvcount = num_count / (2);
		if ((num_count % 2 == 1) && (RowRank > 0))
			recvcount++;
		//receive polinoms, multiply polinoms
		for (int i = 0; i < recvcount; i++)
		{
			if (coord[0] == 1)
			{
				MPI_Recv(inum, 1, polinom, 0, 0, ColComm, &Status);
			}
			else
			{
				MPI_Recv(inum, 1, polinom, 0, 0, RowComm, &Status);
			}
			if (i == 0)
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
		// send to (1,1) proc
		if (RowRank == 1)
		{
			MPI_Send(res, 1, polinom2, ColRank + 1, 0, ColComm);
		}
		if (ColRank == 1)
		{
			MPI_Send(res, 1, polinom2, RowRank + 1, 0, RowComm);
		}
	}
	MPI_Type_free(&polinom);
	MPI_Barrier(MPI_COMM_WORLD);
	MPI_Finalize();
	return 0;

}

#pragma region matrix
void clear(double** arr, int n)
{
	for (int i = 0; i < n; i++)
		delete[] arr[i];
	delete[] arr;
}
double** clone(double** arr, int n)
{
	double** newArr = new double* [n];
	for (int row = 0; row < n; row++)
	{
		newArr[row] = new double[n];
		for (int col = 0; col < n; col++)
			newArr[row][col] = arr[row][col];
	}
	return newArr;
}
void show(double** matrix, int n)
{
	for (int row = 0; row < n; row++) {
		for (int col = 0; col < n; col++)
			printf("%lf\t", matrix[row][col]);
		printf("\n");
	}
	printf("\n");
}
double** matrix_multi(double** A, double** B, int n)
{
	double** result = new double* [n];
	for (int row = 0; row < n; row++) {
		result[row] = new double[n];
		for (int col = 0; col < n; col++) {
			result[row][col] = 0;
		}
	}
	for (int row = 0; row < n; row++) {
		for (int col = 0; col < n; col++) {
			for (int j = 0; j < n; j++) {
				result[row][col] += A[row][j] * B[j][col];
			}
		}
	}
	return result;
}
void scalar_multi(double** m, int n, double a) {
	for (int row = 0; row < n; row++)
		for (int col = 0; col < n; col++) {
			m[row][col] *= a;
		}
}
void sum(double** A, double** B, int n)
{
	for (int row = 0; row < n; row++)
		for (int col = 0; col < n; col++)
			A[row][col] += B[row][col];
}
double det(double** matrix, int n)
{
	double** B = clone(matrix, n);
	for (int step = 0; step < n - 1; step++)
		for (int row = step + 1; row < n; row++)
		{
			double coeff = -B[row][step] / B[step][step];
			for (int col = step; col < n; col++)
				B[row][col] += B[step][col] * coeff;
		}
	double Det = 1;
	for (int i = 0; i < n; i++)
		Det *= B[i][i];
	clear(B, n);
	return Det;
}
double** matrix(double** A, int n)
{
	double N1 = 0, Ninf = 0;
	double** A0 = clone(A, n);
	for (size_t row = 0; row < n; row++) {
		double colsum = 0, rowsum = 0;
		for (size_t col = 0; col < n; col++) {
			rowsum += fabs(A0[row][col]);
			colsum += fabs(A0[col][row]);
		}
		N1 = std::max(colsum, N1);
		Ninf = std::max(rowsum, Ninf);
	}
	for (size_t row = 0; row < n - 1; row++) {
		for (size_t col = row + 1; col < n; col++)
			std::swap(A0[col][row], A0[row][col]);
	}
	scalar_multi(A0, n, (1 / (N1 * Ninf)));
	double** E2 = new double* [n];
	for (int row = 0; row < n; row++)
	{
		E2[row] = new double[n];
		for (int col = 0; col < n; col++) {
			if (row == col)
				E2[row][col] = 2;
			else
				E2[row][col] = 0;
		}
	}
	double** inv = clone(A0, n); 
	double EPS = 1e-11;
	if (det(A, n) != 0) { 
		while (fabs(det(matrix_multi(A, inv, n), n) - 1) >= EPS)
		{
			double** prev = clone(inv, n); 
			inv = matrix_multi(A, prev, n);
			scalar_multi(inv, n, -1);      
			sum(inv, E2, n);               
			inv = matrix_multi(prev, inv, n);
			clear(prev, n);
		}
		show(inv, n);
	}
	else
		printf("Impossible\n");
	clear(A, n);
	clear(E2, n);
	return inv;
}
#pragma endregion

#pragma region fourier
typedef complex<double> base;
void fft(vector<base>& a, bool invert) {
	int n = (int)a.size();
	if (n == 1)  return;

	vector<base> a0(n / 2), a1(n / 2);
	for (int i = 0, j = 0; i < n; i += 2, ++j) {
		a0[j] = a[i];
		a1[j] = a[i + 1];
	}
	fft(a0, invert);
	fft(a1, invert);

	double ang = 2 * M_PI / n * (invert ? -1 : 1);
	base w(1), wn(cos(ang), sin(ang));
	for (int i = 0; i < n / 2; ++i) {
		a[i] = a0[i] + w * a1[i];
		a[i + n / 2] = a0[i] - w * a1[i];
		if (invert)
			a[i] /= 2, a[i + n / 2] /= 2;
		w *= wn;
	}
}
void multiply(const vector<int>& a, const vector<int>& b, vector<int>& res) {
	vector<base> fa(a.begin(), a.end()), fb(b.begin(), b.end());
	size_t n = 1;
	while (n < max(a.size(), b.size()))  n <<= 1;
	n <<= 1;
	fa.resize(n), fb.resize(n);

	fft(fa, false), fft(fb, false);
	for (size_t i = 0; i < n; ++i)
		fa[i] *= fb[i];
	fft(fa, true);

	res.resize(n);
	for (size_t i = 0; i < n; ++i)
		res[i] = int(fa[i].real() + 0.5);
}
#pragma endregion