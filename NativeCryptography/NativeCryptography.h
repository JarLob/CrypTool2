#pragma once
#include "aes_core.h"
#include "DES/des.h"
#include <stdlib.h>

using namespace System;

namespace NativeCryptography {

	public ref class Crypto
	{
	private:
		static void xorBlockAES(int *t1, int *t2)
		{
			t1[0] ^= t2[0];
			t1[1] ^= t2[1];
			t1[2] ^= t2[2];
			t1[3] ^= t2[3];
		}

		static void xorBlockDES(int *t1, int *t2)
		{
			t1[0] ^= t2[0];
			t1[1] ^= t2[1];
		}

	public:
		static array<unsigned char>^ decryptAES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int bits, const int length, const int mode)
		{
			const int blockSize = 16;
			int numBlocks = length / blockSize;
			if (length % blockSize != 0)
				numBlocks++;

			pin_ptr<unsigned char> input = &Input[0];
			pin_ptr<unsigned char> key = &Key[0];
			pin_ptr<unsigned char> iv = &IV[0];

			array<unsigned char>^ output = gcnew array<unsigned char>(length);
			pin_ptr<unsigned char> outp = &output[0];	

			AES_KEY aeskey;			
			if (mode == 2)	//CFB
			{
				AES_set_encrypt_key(key, bits, &aeskey);

				AES_encrypt(iv, outp, &aeskey);
				xorBlockAES((int*)(outp), (int*)(input));

				for (int c = 0; c < numBlocks-1; c++)
				{
					AES_encrypt((input+c*blockSize), outp+(c+1)*blockSize, &aeskey);					
					xorBlockAES((int*)(outp+(c+1)*blockSize), (int*)(input+(c+1)*blockSize));
				}
			}
			else
			{
				AES_set_decrypt_key(key, bits, &aeskey);
				AES_decrypt(input, outp, &aeskey);
				if (mode == 1)		//CBC
					xorBlockAES((int*)(outp), (int*)iv);	
				for (int c = 1; c < numBlocks; c++)
				{
					AES_decrypt((input+c*blockSize), outp+c*blockSize, &aeskey);
					if (mode == 1)		//CBC
						xorBlockAES((int*)(outp+c*blockSize), (int*)(input+(c-1)*blockSize));				
				}
			}

			return output;
		}

		static array<unsigned char>^ decryptDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length, const int mode)
		{
			const int blockSize = 8;
			int numBlocks = length / blockSize;
			if (length % blockSize != 0)
				numBlocks++;

			pin_ptr<unsigned char> input = &Input[0];
			pin_ptr<unsigned char> key = &Key[0];
			pin_ptr<unsigned char> iv = &IV[0];

			array<unsigned char>^ output = gcnew array<unsigned char>(length);
			pin_ptr<unsigned char> outp = &output[0];			

			DES_key_schedule deskey;
			DES_set_key_unchecked((const_DES_cblock*)key, &deskey);

			if (mode == 2)	//CFB
			{				
				DES_ecb_encrypt((const_DES_cblock*)iv, (const_DES_cblock*)outp, &deskey, DES_ENCRYPT);
				xorBlockAES((int*)(outp), (int*)(input));

				for (int c = 0; c < numBlocks-1; c++)
				{
					DES_ecb_encrypt((const_DES_cblock*)(input+c*blockSize), (const_DES_cblock*)(outp+(c+1)*blockSize), &deskey, DES_ENCRYPT);
					xorBlockDES((int*)(outp+(c+1)*blockSize), (int*)(input+(c+1)*blockSize));
				}
			}
			else
			{
				DES_ecb_encrypt((const_DES_cblock*)input, (const_DES_cblock*)outp, &deskey, DES_DECRYPT);
				if (mode == 1)		//CBC
					xorBlockDES((int*)(outp), (int*)iv);	
				for (int c = 1; c < numBlocks; c++)
				{
					DES_ecb_encrypt((const_DES_cblock*)(input+c*blockSize), (const_DES_cblock*)(outp+c*blockSize), &deskey, DES_DECRYPT);
					if (mode == 1)		//CBC
						xorBlockDES((int*)(outp+c*blockSize), (int*)(input+(c-1)*blockSize));
				}
			}

			return output;
		}
	};
}