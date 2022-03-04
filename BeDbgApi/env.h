#pragma once

namespace BeDbgApi::Env
{
	enum class Platform
	{
		X86,
		AMD64 // 🤮 X64 is a macro used in windows libs
	};

	enum class Configuration
	{
		DEBUG,
		RELEASE
	};
}
