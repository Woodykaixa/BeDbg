#include "path.h"

#include <string>

#include "error.h"


bool BeDbgApi::Path::GetPathFromFileHandle(Type::sys_handle_t fileHandle,
                                           _Out_writes_(size) wchar_t* pathBuffer,
                                           const std::uint32_t size)
{
    const auto sizeWritten = GetFinalPathNameByHandleW(fileHandle, pathBuffer, size, 0);
    if (sizeWritten == 0)
    {
        *Error::GetInnerError() = Error::Error{
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = L"GetPathFromFileHandle failed",
        };
        return false;
    }
    if (size < sizeWritten)
    {
        *Error::GetInnerError() = Error::Error{
            .exceptionModule = Error::ExceptionModule::SYSTEM,
            .code = GetLastError(),
            .message = L"GetPathFromFileHandle buffer too small",
        };
        return false;
    }

    if (_wcsnicmp(pathBuffer, L"\\\\?\\UNC\\", 8) != 0)
    {
        if (_wcsnicmp(pathBuffer, L"\\\\?\\", 4) || pathBuffer[5] != L':')
        {
            wcsncpy_s(pathBuffer, size, pathBuffer + 4, _TRUNCATE);
        }
    }
    else
    {
        wcsncpy_s(pathBuffer, size, L"\\\\", _TRUNCATE);
        wcsncat_s(pathBuffer, size, pathBuffer + 8, _TRUNCATE);
    }
    std::wstring utf8 = pathBuffer;
    if (utf8.rfind(LR"(\Device\vmsmb\VSMB-)", 0) == 0)
    {
        if (const auto windowsIdx = utf8.find(LR"(\os\Windows\)");
            windowsIdx != std::string::npos)
        {
            utf8 = LR"(C:\)" + utf8.substr(windowsIdx + 4);
        }
    }

    if (utf8.rfind(LR"(\Device\)", 0) == 0)
    {
        // CreateFileW does not work on \Device\ paths directly, you need to prepend this
        utf8.insert(0, LR"(\\?\GLOBALROOT)");
    }
    wcsncpy_s(pathBuffer, size, utf8.c_str(), _TRUNCATE);
    return true;
}
