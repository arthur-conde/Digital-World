//http://somebastardstolemyname.wordpress.com
//The_Undead : Rhys M.

//winsock send hook

#include "windows.h"
#include "winsock.h"

#pragma comment ( lib, "Ws2_32.lib" )
#define JMP(frm, to) (int)(((int)to - (int)frm) - 5);

DWORD SendOriginalAddress = 0;
DWORD SendReturnAddress = 0;
DWORD* SendNewAddress = 0;
DWORD OldProtection = 0;

char* send_buffer;
int send_sizeofdata = 0;
SOCKET send_s;
int send_flags = 0;

int (__stdcall* osend)(SOCKET s, const char* buffer, int len, int flags) = send;

int WINAPI __stdcall msend(SOCKET s, const char* buffer, int len, int flags)
{
	MessageBox(NULL, "msend", "msend", MB_OK);
	return osend(s,buffer,len,flags);
}

void __declspec(naked) __stdcall  SendHookFunc() 	
{
	__asm
	{ 
				mov  edi,edi
				push ebp
				mov ebp, esp
				mov eax, [ebp+0x08] /* Param 1 : Socket */
				mov send_s, eax
				mov eax, [ebp+0x0C] /* Param 2 : buffer */
				mov [send_buffer], eax
				mov eax, [ebp+0x10] /*Param 3 : Size*/
				mov send_sizeofdata, eax
				mov eax, [ebp+0x14] /*Param 4 : flags*/
				mov send_flags, eax
				jmp SendReturnAddress
	}
}

void UnHookSend()
{
	/* To unhook on a WinXP post SP2 box you need to restore the 5 byte preamble */
	*(WORD *)SendOriginalAddress = 0xFF8B;		// mov  edi,edi
	*(BYTE *)(SendOriginalAddress+2) = 0x55;	// push epb
	*(WORD *)(SendOriginalAddress+3) = 0xEC8B;	// mov epb, esp
	VirtualProtect( (void*)SendOriginalAddress, 0x05, OldProtection, &OldProtection );
}

void HookSend()
{
	SendNewAddress = (DWORD*)msend;
	HINSTANCE hDll = LoadLibrary((LPCTSTR) "Ws2_32.dll"); 
	SendOriginalAddress = (DWORD)GetProcAddress(hDll, (LPCSTR)19); 
	SendReturnAddress = SendOriginalAddress + 5;
	VirtualProtect( (void*)SendOriginalAddress, 0x05, PAGE_READWRITE , &OldProtection );
	*(BYTE *)(SendOriginalAddress) = 0xe9;
	*(int *)(SendOriginalAddress+1) = JMP(SendOriginalAddress, SendNewAddress);
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
		HookSend();
	if (ul_reason_for_call == DLL_THREAD_DETACH)
		UnHookSend();
    return TRUE;
}