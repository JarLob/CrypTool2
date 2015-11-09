#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include <stdio.h>
#include <curand.h> 
#include <curand_kernel.h>


__global__ void kernelENG(long totalThreads, int* ciphertext, int textLength, int* runkey,
						double* quadgrams, double* cuda_out)
{
	int x = blockIdx.x*blockDim.x+threadIdx.x;
	int y = blockIdx.y*blockDim.y+threadIdx.y;
	int index = x + y * blockDim.x;    //ThreadID. IMPORTANT: Build blocks (X*Y) with X=Y  !!! 2x2, 3x3 ...

	int plaintext[1000];	// 10000Must have constant Value, [textLength[0]] not possible. IMPORTANT: There wont be more then 10k Symbols loaded into kernel. Handeled in c# Code (HillclimbingAttacker).
	int i = index / 26;		// With i and j the Algorithm Computes the Chiddkey(K*). See the Modifyblock 
	int j = index % 26;
	

	int temp;
	double costvalue = 0;
	int threadKey[26];


	for (int k = 0; k < 26; k++)
	{
		threadKey[k] = runkey[k];
	}

	//K* = Modify K by swap position i and j
	temp = threadKey[i];
	threadKey[i] = threadKey[j];
	threadKey[j] = temp;

	//Plain = cipher, K*
	for (int k = 0; k < textLength; k++)
	{
		plaintext[k] = threadKey[ciphertext[k]];
	}

	//Costfunction
	int end = textLength -3;
	for (int k = 0; k < end; k++)
	{
		costvalue +=  quadgrams[plaintext[k] + (plaintext[k + 1] * 26) +
			(plaintext[k + 2]*26*26) +  (plaintext[k + 3]*26*26*26)];
	}

	//Output Return the Costvalue for each Thread
	for (int k = 0; k < totalThreads; k++)
	{
		cuda_out[index] = costvalue;
	}

}

__global__ void kernelGER(long totalThreads, int* ciphertext, int textLength, int* runkey,
						double* quadgrams, double* cuda_out)
{
	int x = blockIdx.x*blockDim.x+threadIdx.x;
	int y = blockIdx.y*blockDim.y+threadIdx.y;
	int index = x + y * blockDim.x;   //ThreadID. IMPORTANT: Build blocks (X*Y) with X=Y  !!! 2x2, 3x3 ...

	int plaintext[10000];	// Must have constant Value, [textLength[0]] not possible. IMPORTANT: There wont be more then 10k Symbols loaded into kernel. Handeled in c# Code (HillclimbingAttacker).
	int i = index / 30;		// With i and j the Algorithm Computes the Chiddkey(K*). See the Modifyblock 
	int j = index % 30;
	int temp;
	double costvalue = 0;
	int threadKey[30];

	for (int k = 0; k < 30; k++)
	{
		threadKey[k] = runkey[k];
	}

	//K* = Modify K by swap position i and j
	temp = threadKey[i];
	threadKey[i] = threadKey[j];
	threadKey[j] = temp;

	//Plain = cipher, K*
	for (int k = 0; k < textLength; k++)
	{
		plaintext[k] = threadKey[ciphertext[k]];
	}

	//Costfunction
	int end = textLength -3;	
	for (int k = 0; k < end; k++)
	{
		costvalue +=  quadgrams[plaintext[k] + (plaintext[k + 1] * 30) +
			(plaintext[k + 2]*30*30) +  (plaintext[k + 3]*30*30*30)];
	}

	//Output Return the Costvalue for each Thread
	for (int k = 0; k < totalThreads; k++)
	{
		cuda_out[index] = costvalue;
	}
}


int main()
{
    return 0;
}
