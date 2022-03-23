#pragma once
#include <cstdint>
#include "defines.h"

namespace BeDbgApi::Process
{
    BEDBG_API Type::handle_t AttachProcess(int pid);
    /**
     * @brief Start a process in debug mode.
     */
    BEDBG_API std::uint32_t StartProcess(const wchar_t* filename, const wchar_t* command, wchar_t* environment,
                                         const wchar_t* workingDirectory);
    BEDBG_API bool DetachProcess(Type::handle_t handle);
    BEDBG_API bool IsAttachableProcess(int pid);
}
