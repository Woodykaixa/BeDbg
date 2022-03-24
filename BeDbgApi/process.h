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

    constexpr std::uint32_t PROCESS_NAME_SIZE = 260;

    struct ProcessModuleInformation
    {
        wchar_t name[PROCESS_NAME_SIZE];
        std::uint64_t entry;
        std::uint32_t size;
        std::uint64_t imageBase;
    };

    BEDBG_API bool QueryProcessModules(Type::handle_t handle,
                                       _Inout_count_(sizeof(ProcessModuleInformation)*count) ProcessModuleInformation
                                       modules[], size_t count, size_t* usedCount);
}
