<template>
  <div>DebugView: PID: {{ process.id }}</div>
  <div @click="stopDebug">stop debug</div>
</template>

<script setup lang="ts">
import { effect, onBeforeUnmount, onMounted, onUnmounted, reactive, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useNotification } from 'naive-ui';
import { Api } from '@/api';
import { DebuggingProcess } from '@/dto/process';

const router = useRouter();
const notification = useNotification();
const process = reactive<DebuggingProcess>({ id: 0, handle: 0, attachTime: new Date() });
async function CheckDebugging() {
  const { data, ok } = await Api.DebuggingProcess.list();
  if (ok) {
    if (data.length === 0) {
      notification.error({
        title: '未指定调试进程',
        content: '请附加或创建一个进程来调试',
      });
      router.push('/');
      return;
    }
    process.id = data[0].id;
    process.attachTime = data[0].attachTime;
    process.handle = data[0].handle;
  } else {
    notification.error({
      title: '无法获取调试中进程',
      description: data.error,
      content: data.message,
    });
    router.push('/');
    return;
  }
}

onMounted(() => {
  CheckDebugging();
});

const stopDebug = async () => {
  sessionStorage.removeItem('debugPid');
  const { ok, data } = await Api.DebuggingProcess.detachProcess(process.id);
  if (ok) {
    router.push('/');
  } else {
    notification.error({
      title: '退出调试失败',
      description: data.error,
      content: data.message,
    });
  }
};

effect(async () => {
  const resp = await Api.readProcessMemory({ pid: process.id, address: 0, size: 0 })!;
  console.log(resp, typeof resp);
});
</script>
