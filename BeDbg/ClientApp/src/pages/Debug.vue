<template>
  <div>DebugView: PID: {{ pid }}</div>
  <div @click="stopDebug">stop debug</div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, onUnmounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useNotification } from 'naive-ui';

const router = useRouter();
const notification = useNotification();
const pid = parseInt(sessionStorage.getItem('debugPid') ?? '0', 10);
if (pid === 0) {
  notification.error({
    title: '未指定调试进程',
    content: '请附加或创建一个进程来调试',
  });
  router.push('/');
}

const stopDebug = () => {
  sessionStorage.removeItem('debugPid');
  router.push('/');
};
</script>
