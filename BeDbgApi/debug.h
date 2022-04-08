#pragma once
#include "defines.h"

#include  <cstdint>

#include <Windows.h>

namespace BeDbgApi::Debug
{
    enum class DebugContinueStatus : bool
    {
        Continue = true,
        NotHandled = false
    };

    template <typename TInfo = decltype(DEBUG_EVENT::u)>
    using DebugEventCallback = DebugContinueStatus (__stdcall *)(std::uint32_t pid, std::uint32_t tid,
                                                                 const TInfo* info);

    struct DebugLoopCallbacks
    {
        DebugEventCallback<EXCEPTION_DEBUG_INFO> OnException;
        DebugEventCallback<CREATE_THREAD_DEBUG_INFO> OnCreateThread;
        DebugEventCallback<CREATE_PROCESS_DEBUG_INFO> OnCreateProcess;
        DebugEventCallback<EXIT_THREAD_DEBUG_INFO> OnExitThread;
        DebugEventCallback<EXIT_PROCESS_DEBUG_INFO> OnExitProcess;
        DebugEventCallback<LOAD_DLL_DEBUG_INFO> OnLoadDll;
        DebugEventCallback<LOAD_DLL_DEBUG_INFO> OnUnloadDll;
        DebugEventCallback<OUTPUT_DEBUG_STRING_INFO> OnOutputDebugString;
        DebugEventCallback<RIP_INFO> OnRip;
    };


    BEDBG_API Type::handle_t<DebugLoopCallbacks> CreateDebugLoopCallbacks();
    BEDBG_API void SetDebugLoopCallback(Type::handle_t<DebugLoopCallbacks> callbacks, int eventId,
                                        DebugEventCallback<> callback);
    BEDBG_API void DestroyDebugLoopCallbacks(Type::handle_t<DebugLoopCallbacks>);

    namespace Internal
    {
        TEST_ONLY_API DebugContinueStatus dispatchDebugEvent(const DEBUG_EVENT* event,
                                                             Type::handle_t<DebugLoopCallbacks> callbacks);
    }

    BEDBG_API DebugContinueStatus DebugLoopWaitEvent(Type::handle_t<DebugLoopCallbacks> callbacks);
}
