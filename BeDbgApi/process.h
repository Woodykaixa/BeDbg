#pragma once
#include <cstddef>
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

    BEDBG_API bool DumpAssembly(Type::handle_t handle);

    constexpr std::uint32_t BUFFER_SIZE = 260;

    struct ProcessModuleInformation
    {
        wchar_t name[BUFFER_SIZE];
        std::uint64_t entry;
        std::uint32_t size;
        std::uint64_t imageBase;
    };

    BEDBG_API bool QueryProcessModules(Type::handle_t handle,
                                       _Out_writes_(sizeof(ProcessModuleInformation)*count) ProcessModuleInformation
                                       modules[], size_t count, size_t* usedCount);

    struct ProcessMemoryBlockInformation
    {
        std::uint64_t baseAddress;
        std::uint64_t allocAddress;
        std::uint64_t size;
        std::uint32_t protectionFlags;
        std::uint32_t initialProtectionFlags;
        std::uint32_t state;
        std::uint32_t type;
        wchar_t info[BUFFER_SIZE];
    };


    BEDBG_API _Success_(return > 0) size_t QueryProcessMemoryInfos(Type::handle_t handle,
                                                                   _Out_writes_(
                                                                       sizeof(ProcessMemoryBlockInformation)* count)
                                                                   ProcessMemoryBlockInformation infos[], size_t count);
}
