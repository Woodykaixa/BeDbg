<script setup lang="ts">
import { effect, onMounted, reactive, ref, PropType, Component, h, inject } from 'vue';
import { useRouter } from 'vue-router';
import {
  useNotification,
  NList,
  NLi,
  NScrollbar,
  NCollapse,
  NCollapseItem,
  NCode,
  NLayout,
  NLayoutSider,
  NLayoutContent,
  NButton,
} from 'naive-ui';
import { DataFormatter } from '@/util/formatter';
import { DebuggerEventSource } from '@/util/debuggerEventSource';
import { useDebuggerEventSource } from '@/hooks/useDebuggerEvent';
import type { ProcessModule, ProcessMemoryPage } from '@/dto/process';
import { Api } from '@/api';
import type { ErrorResponse } from '@/dto/error';

const props = defineProps({
  pid: {
    type: Number,
    required: true,
  },
});

const debuggerEvent = useDebuggerEventSource();
const process = reactive({
  modules: [] as ProcessModule[],
  pages: [] as (ProcessMemoryPage & { data: string })[],
});

const notification = useNotification();
const router = useRouter();

debuggerEvent.addEventListener('programReady', async () => {
  const { data: modules, ok: moduleOk } = await Api.DebuggingProcess.listModules(props.pid);
  const { data: pages, ok: pageOk } = await Api.DebuggingProcess.listPages(props.pid);

  if (!moduleOk || !pageOk) {
    const err = (moduleOk ? pages : modules) as ErrorResponse;
    notification.error({
      title: '无法获取进程模块',
      description: err.error,
      content: err.message,
    });
    router.push('/');
    return;
  }
  const map = new Map<number, ProcessModule>();
  modules.forEach(m => {
    map.set(m.base, m);
  });
  process.modules = modules.sort((a, b) => a.base - b.base);
  process.pages = pages
    .sort((a, b) => a.baseAddress - b.baseAddress)
    .map(m => {
      const module = map.get(m.baseAddress);
      if (module) {
        return {
          ...m,
          data: module.name,
        };
      }
      return {
        ...m,
        data: '',
      };
    });
});

const fakeReg = [
  { name: 'RAX', value: 0x34 },
  { name: 'RBX', value: 0 },
  { name: 'RCX', value: 0x00000096892ffa40 },
  { name: 'RDX', value: 0xa },
  { name: 'RBP', value: 0 },
  { name: 'RSP', value: 0x00000096892ffa18 },
  { name: 'RSI', value: 0 },
  { name: 'RDI', value: 0 },
  { name: 'R8', value: 0 },
  { name: 'R9', value: 0 },
  { name: 'R10', value: 0 },
  { name: 'R11', value: 0x0000000000000246 },
  { name: 'R12', value: 0 },
  { name: 'R13', value: 0 },
  { name: 'R14', value: 0 },
  { name: 'R15', value: 0 },
  { name: 'RIP', value: 0x00007ffeba6ed3f4 },
];
</script>

<template>
  <n-scrollbar>
    <n-collapse>
      <n-collapse-item title="载入模块">
        <div class="memory-layout">
          <div class="name">模块名称</div>
          <div class="entry">入口地址</div>
          <div class="size">模块大小</div>
          <div class="base">装载地址</div>
        </div>
        <code v-for="m in process.modules" class="memory-layout">
          <div class="name" :title="m.name">{{ m.name.slice(m.name.lastIndexOf('\\') + 1) }}</div>
          <div class="entry">{{ DataFormatter.formatNumberHex(m.entry) }}</div>
          <div class="size">{{ DataFormatter.formatNumberHex(m.size) }}</div>
          <div class="base">{{ DataFormatter.formatNumberHex(m.base) }}</div>
        </code>
      </n-collapse-item>
      <n-collapse-item title="进程内存">
        <div class="memory-page-layout">
          <div class="address">地址</div>
          <div class="address">大小</div>
          <div class="flags">页面信息</div>
          <div class="flags">类型</div>
          <div class="flags">页面保护</div>
          <div class="flags">初始保护</div>
        </div>
        <code v-for="m in process.pages" class="memory-page-layout">
          <div class="address">{{ DataFormatter.formatNumberHex(m.baseAddress) }}</div>
          <div class="address">{{ DataFormatter.formatNumberHex(m.size) }}</div>
          <div class="flags">{{ m.data }}</div>
          <div class="flags">{{ m.type === 0x1000000 ? 'IMAGE' : m.type === 0x40000 ? 'MAPPED' : 'PRIVATE' }}</div>
          <div class="flags">
            {{ DataFormatter.formatProtectionFlag(m.flags) }}
          </div>
          <div class="flags">
            {{ DataFormatter.formatProtectionFlag(m.initialFlags) }}
          </div>
        </code>
      </n-collapse-item>
      <n-collapse-item title="寄存器">
        <div class="register-box" :style="{ display: 'flex', 'flex-wrap': 'wrap' }">
          <div v-for="reg in fakeReg" class="register">
            <code class="name">{{ reg.name }}</code>
            <code class="value">{{ DataFormatter.formatNumberHex(reg.value) }}</code>
          </div>
        </div>
      </n-collapse-item>
      <n-collapse-item title="其他功能">
        <n-button> 结束调试 </n-button>
      </n-collapse-item>
    </n-collapse>
  </n-scrollbar>
</template>

<style scoped>
.memory-layout {
  display: flex;
  width: 100%;
}

.memory-layout .entry,
.memory-layout .base,
.memory-layout .size,
.memory-layout .name {
  width: 25%;
}

.memory-page-layout {
  display: flex;
  width: 100%;
  flex-wrap: nowrap;
}

.memory-page-layout .address {
  width: 20%;
}

.memory-page-layout .flags {
  width: 15%;
}

.register-box {
  margin-left: 4px;
}

.register-box .register {
  box-sizing: border-box;
  padding: 4px;
  display: flex;
}

.register-box .register .name {
  width: 30px;
  margin: 0 4px;
}

.register-box .register .value {
}

.data-panel {
  width: 30%;
  display: flex;
  flex-direction: column;
}
</style>
