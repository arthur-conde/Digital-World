// SRand.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

using namespace std;


const int limit = 32000;

short readShort(short& res, const unsigned char* bytes, int pos)
{
	res = 0;
	for(int n = pos + 2; n >= pos; n--)
		res = (res << 8) + bytes[n];
	return res;
}

void doStuff1()
{
	int time1, res1,time2, res2;
	cout << "Enter the first seed: ";
	cin >> time1;

	cout << "Enter the first desired result: ";
	cin >> res1;

	cout << "Enter the second seed: ";
	cin >> time2;

	cout << "Enter the second desired result: ";
	cin >> res2;

	cout << "\n";

	srand(time1);
	byte buffer1[10000];
	byte buffer2[10000];
	for(int i = 0; i < 10000; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer1[i] = (byte)(rand() % 256);
		//printf("%02X ", buffer1[i]);
	}
	srand(time2);
	for(int i = 0; i < 10000; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer2[i] = (byte)(rand() % 256);
	}
	for(int i = 0; i < 10000; i++)
	{
		byte short1 = buffer1[i];
		byte short2 = buffer2[i];
		//short1 = readShort(short1, buffer1, 2*i);
		//short2 = readShort(short2, buffer2, 2*i);

		for(int j = 1; j <= 255; j++)
		{
			if ((short1 ^ (byte)j) == res1 && (short2 ^ (byte)j) == res2)
			{
				printf("idx: %02X [%02X ^ %02X = %02X = %02X] = [%02X ^ %02X = %02X = %02X]\n", 
					i,
					short1,j,(short1 ^ (byte)j),res1,
					short2,j,(short2 ^ (byte)j),res2);
			}
		}
	}
}

void doStuff4()
{
	int time1, res1,time2, res2, time3, res3;
	cout << "Enter the first seed: ";
	cin >> time1;

	cout << "Enter the first desired result: ";
	cin >> res1;

	cout << "Enter the second seed: ";
	cin >> time2;

	cout << "Enter the second desired result: ";
	cin >> res2;

	cout << "Enter the third seed: ";
	cin >> time3;

	cout << "Enter the third desired result: ";
	cin >> res3;

	cout << "\n";

	srand(time1);

	byte buffer1[limit];
	byte buffer2[limit];
	byte buffer3[limit];
	for(int i = 0; i < limit; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer1[i] = (byte)(rand() % 256);
		//printf("%02X ", buffer1[i]);
	}
	srand(time2);
	for(int i = 0; i < limit; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer2[i] = (byte)(rand() % 256);
	}
	srand(time3);
	for(int i = 0; i < limit; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer3[i] = (byte)(rand() % 256);
	}

	for(int i = 0; i < limit; i++)
	{
		byte short1 = buffer1[i];
		byte short2 = buffer2[i];
		byte short3 = buffer3[i];
		//short1 = readShort(short1, buffer1, 2*i);
		//short2 = readShort(short2, buffer2, 2*i);

		for(int j = 1; j <= 255; j++)
		{
			if ((short1 ^ (byte)j) == res1 && (short2 ^ (byte)j) == res2 && (short3 ^ (byte)j) == res3)
			{
				printf("idx: %02X [%02X ^ %02X = %02X = %02X] = [%02X ^ %02X = %02X = %02X] = [%02X ^ %02X = %02X = %02X]\n", 
					i,
					short1,j,(short1 ^ (byte)j),res1,
					short2,j,(short2 ^ (byte)j),res2,
					short3,j,(short3 ^ (byte)j),res3
					);
			}
		}
	}
}

void doStuff5()
{
	int time1, res1,time2, res2, time3, res3;
	cout << "Enter the first seed: ";
	cin >> time1;

	cout << "Enter the first desired result: ";
	cin >> res1;

	cout << "Enter the second seed: ";
	cin >> time2;

	cout << "Enter the second desired result: ";
	cin >> res2;

	cout << "Enter the third seed: ";
	cin >> time3;

	cout << "Enter the third desired result: ";
	cin >> res3;

	cout << "\n";

	srand(time1);

	byte buffer1[limit];
	byte buffer2[limit];
	byte buffer3[limit];
	for(int i = 0; i < limit; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer1[i] = (byte)(rand() % 256);
		//printf("%02X ", buffer1[i]);
	}
	srand(time2);
	for(int i = 0; i < limit; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer2[i] = (byte)(rand() % 256);
	}
	srand(time3);
	for(int i = 0; i < limit; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer3[i] = (byte)(rand() % 256);
	}

	for(int i = 0; i < limit; i++)
	{
		byte short1 = buffer1[i];
		//short1 = readShort(short1, buffer1, 2*i);
		//short2 = readShort(short2, buffer2, 2*i);
			if (short1  == res1 )
			{
				printf("idx: %02X || ", 
					i,
					short1,res1
					);
				break;
		}
	}
			for(int i = 0; i < limit; i++)
	{
		byte short1 = buffer2[i];
		//short1 = readShort(short1, buffer1, 2*i);
		//short2 = readShort(short2, buffer2, 2*i);
			if (short1  == res1 )
			{
				printf("idx: %02X || ", 
					i,
					short1,res1
					);
				break;
		}
	}
	for(int i = 0; i < limit; i++)
	{
		byte short1 = buffer1[i];
		//short1 = readShort(short1, buffer1, 2*i);
		//short2 = readShort(short2, buffer2, 2*i);
			if (short1  == res1 )
			{
				printf("idx: %02X\n", 
					i,
					short1,res1
					);
				break;
		}
	}
}

void doStuff3()
{
	FILE* fs;
	fs = fopen("henshin.txt","w");
	int time1, res1;
	cout << "Enter the first seed: ";
	cin >> time1;

	cout << "Enter the first desired result: ";
	cin >> res1;

	cout << "\n";

	srand(time1);
	byte buffer1[10000];
	for(int i = 0; i < 10000; i++)
	{
		//if (i > 0 && i % 16 == 0) printf("\n");
		buffer1[i] = (byte)(rand() % 256);
		//printf("%02X ", buffer1[i]);
	}
	for(int i = 0; i < 10000; i++)
	{
		byte short1 = buffer1[i];
		//short1 = readShort(short1, buffer1, 2*i);
		//short2 = readShort(short2, buffer2, 2*i);

		for(int j = 1; j <= 255; j++)
		{
			if ((short1 ^ (byte)j) == res1 )
			{
				fprintf(fs,"idx: %0d %02X [%02X ^ %02X = %02X = %02X]\n", i, i,
					short1,j,(short1 ^ (byte)j),res1);
			}
		}
	}
	fclose(fs);
}

void doStuff2()
{
	byte b1 = 0xde;
	byte r1 = 0x27;
	byte b2 = 0x56	;
	byte r2 = 0x1d;
	for(int i = 0; i <= 255; i++)
	{
		if (((byte)i ^ b1) == r1 && ((byte)i ^ b2) == r2)
		{
			printf("idx: %d %02X ^ %02X = %02X | %02X ^ %02X = %02X\n", i,
				i, b1, (byte)i ^ b1,
				i, b2, (byte)i ^ b2);
		}
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	doStuff5();	
	cout << "Done.\n";

	string stuff = "";
	while(stuff != "exit")
	{
		cin >> stuff;
		cin.ignore();
	}
	return 0;
}

