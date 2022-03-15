#pragma once
#include "defines.h"

namespace BeDbgApi::Process
{
    BEDBG_API Type::handle_t AttachProcess(int pid);
    BEDBG_API bool DetachProcess(Type::handle_t handle);
    BEDBG_API bool IsAttachableProcess(int pid);
}
