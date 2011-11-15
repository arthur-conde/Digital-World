#include "windows.h"
#include "winsock.h"
#include <tchar.h>
#include <detours15.h>

#pragma warning(disable:4100)
#pragma comment(lib,"detours15.lib")
#pragma comment(lib,"wsock32.lib")

/*int (__stdcall *osend)(SOCKET s, const char* buffer, int len, int flags);
int (__stdcall *orecv)(SOCKET s, char* buffer, int len, int flags);*/

DETOUR_TRAMPOLINE(int WINAPI r_send(SOCKET s, const char* buffer, int len, int flags), send);
DETOUR_TRAMPOLINE(int WINAPI r_recv(SOCKET s, char* buffer, int len, int flags), recv);

int WINAPI msend(SOCKET s, const char* buffer, int len, int flags)
{
	return r_send(s, buffer, len, flags);
}

int WINAPI mrecv(SOCKET s, char* buffer, int len, int flags)
{
	return r_recv(s, buffer, len, flags);
}

BOOL APIENTRY DllMain(HANDLE hDll, DWORD reason, LPVOID)
{
	switch(reason)
	{
		case DLL_PROCESS_ATTACH:
		{
			MessageBox(NULL, "DLL_PROCESS_ATTACH","DllMain",MB_OK);
			DetourFunctionWithTrampoline((PBYTE)r_recv,(PBYTE)mrecv);
            DetourFunctionWithTrampoline((PBYTE)r_send,(PBYTE)msend);
			break;
		}
		case DLL_THREAD_DETACH:
		{
			MessageBox(NULL, "DLL_THREAD_DETACH","DllMain",MB_OK);
			DetourRemove((PBYTE)r_recv,(PBYTE)mrecv);
            DetourRemove((PBYTE)r_send,(PBYTE)msend);
			break;
		}
	}
	return true;
}
