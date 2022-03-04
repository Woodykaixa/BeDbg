#pragma once

#include "env.h"
#define EXPORT __declspec(dllexport)
#define BEDBG_API extern "C" EXPORT

namespace BeDbgApi::Env
{
#ifdef WIN32
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
    using handle_t = void*;
    static_assert(sizeof(handle_t) == (Env::IS_WIN32 ? 4 : 8), "pointer size error");
}
