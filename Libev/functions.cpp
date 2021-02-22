#define _USE_MATH_DEFINES

#include <cmath>
#include <algorithm>
#include <stdio.h>
#include <iostream>
#include <time.h>
#include <math.h>
#include <string>
#include <vector>
#include <complex>
using namespace std;
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
		//show(inv, n);
	}
	else
		printf("Impossible\n");
	clear(A, n);
	clear(E2, n);
	return inv;
}

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
