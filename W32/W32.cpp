#include <windows.h>
#include <detours.h>
#include <stdio.h>

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "detours.lib")

int (WINAPI *pSend)(SOCKET s, const char* buf, int len, int flags) = NULL;
int WINAPI MySend(SOCKET s, const char* buf, int len, int flags);
int (WINAPI *pRecv)(SOCKET s, char* buf, int len, int flags) = NULL;
int WINAPI MyRecv(SOCKET s, char* buf, int len, int flags);

int WINAPI MySend(SOCKET s, const char *buf, int len, int flags){
	//MessageBox(NULL, buf, "send", MB_OK);
	printf("Sent:\n%s\n", buf);
	
	return pSend(s, buf, len, flags);
}

int WINAPI MyRecv(SOCKET s, char *buf, int len, int flags){
	//MessageBox(NULL, buf, "recv", MB_OK);
	printf("Sent:\n%s\n", buf);
	return pRecv(s, buf, len, flags);
}

BOOL APIENTRY DllMain(HANDLE hModule, DWORD ul_reason_for_call, LPVOID lpReserved){
	switch(ul_reason_for_call){
		case DLL_PROCESS_ATTACH:
			AllocConsole();
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
			DetourTransactionBegin();
			DetourUpdateThread(GetCurrentThread());
			DetourDetach(&(PVOID&)pSend, MySend);
			DetourDetach(&(PVOID&)pRecv, MyRecv);
			DetourTransactionCommit();
			break;
	}

	return TRUE;
}