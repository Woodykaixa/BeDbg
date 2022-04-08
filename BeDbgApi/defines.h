#pragma once

#include "env.h"

#define EXPORT __declspec(dllexport)
#define BEDBG_API extern "C" EXPORT

#ifndef NDEBUG
#define TEST_ONLY_API BEDBG_API
#elif
#define TEST_ONLY_API  extern "C"
#endif


namespace BeDbgApi::Env
{
#ifndef _WIN64
    constexpr Platform PLATFORM = Platform::X86;
#else
    constexpr Platform PLATFORM = Platform::AMD64;
#endif

#ifndef NDEBUG
    constexpr Configuration CONFIGURATION = Configuration::DEBUG;
#else
	constexpr Configuration CONFIGURATION = Configuration::RELEASE;
#endif

    constexpr bool IS_DEBUG = CONFIGURATION == Configuration::DEBUG;
    constexpr bool IS_WIN32 = PLATFORM == Platform::X86;
}

namespace BeDbgApi::Type
{
    template <typename T>
    using handle_t = T*;

    using sys_handle_t = handle_t<void>;

    static_assert(sizeof(sys_handle_t) == (Env::IS_WIN32 ? 4 : 8), "pointer size error");
}
