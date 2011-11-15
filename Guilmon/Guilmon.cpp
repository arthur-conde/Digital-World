#include <iostream>
#include <fstream>
#include <string>
#include <sstream>
#include <windows.h>
#include <detours.h>

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "detours.lib")

using namespace std;

int (WINAPI *pSend)(SOCKET s, const char* buf, int len, int flags) = send;
int WINAPI MySend(SOCKET s, const char* buf, int len, int flags);
int (WINAPI *pRecv)(SOCKET s, char* buf, int len, int flags) = recv;
int WINAPI MyRecv(SOCKET s, char* buf, int len, int flags);

#define BUFSIZE 8192

HANDLE hRecv;
HANDLE hSend;
DWORD  cbRead, cbToWrite, cbWritten1, cbWritten2, dwMode;
LPTSTR sPipeR = TEXT("\\\\.\\pipe\\guilmon\\recv");
LPTSTR sPipeS = TEXT("\\\\.\\pipe\\guilmon\\send");

fstream fSend;
fstream fRecv;

int WINAPI MySend(SOCKET s, const char *buf, int len, int flags){
	//MessageBox(NULL, buf, "send", MB_OK);
	len = pSend(s, buf, len, flags);
	if (len > 0)
	{
		fSend.write(buf, len);
		fSend.flush();
	}
	return len;
}

int WINAPI MyRecv(SOCKET s, char *buf, int len, int flags){
	//MessageBox(NULL, buf, "recv", MB_OK);
	len = pRecv(s,buf,len,flags);
	if (len > 0)
	{
		fRecv.write(buf, len);
		fRecv.flush();
	}
	return len;
}

BOOL APIENTRY DllMain(HANDLE hModule, DWORD ul_reason_for_call, LPVOID lpReserved){
	switch(ul_reason_for_call){
		case DLL_PROCESS_ATTACH:
			WaitNamedPipe(sPipeR, 0xFFFFFFFF);
			fRecv.open(sPipeR);
			if (!fRecv)
			{
				MessageBox(NULL, TEXT("Failed to connect to Recv"), TEXT("ERROR"), MB_OK);
				return FALSE;
			}

			WaitNamedPipe(sPipeS, 0xFFFFFFFF);
			fSend.open(sPipeS);
			if (!fSend)
			{
				MessageBox(NULL, TEXT("Failed to connect to Send"), TEXT("ERROR"), MB_OK);
				return FALSE;
			}

			pSend = (int (WINAPI *)(SOCKET, const char*, int, int))	DetourFindFunction("Ws2_32.dll", "send");
			pRecv = (int (WINAPI *)(SOCKET, char*, int, int)) DetourFindFunction("Ws2_32.dll", "recv");

			DetourTransactionBegin();
			DetourUpdateThread(GetCurrentThread());
            DetourAttach(&(PVOID&)pSend, MySend);
            if(DetourTransactionCommit() == NO_ERROR)
                printf("send() detoured successfully");
            DetourTransactionBegin();
            DetourUpdateThread(GetCurrentThread());
            DetourAttach(&(PVOID&)pRecv, MyRecv);
            if(DetourTransactionCommit() == NO_ERROR)
                printf("recv() detoured successfully");
            break;

		case DLL_PROCESS_DETACH:
			if (fSend) fSend.close();
			if (fRecv) fRecv.close();
			DetourTransactionBegin();
			DetourUpdateThread(GetCurrentThread());
			DetourDetach(&(PVOID&)pSend, MySend);
			DetourDetach(&(PVOID&)pRecv, MyRecv);
			DetourTransactionCommit();
			break;
	}

	return TRUE;
}