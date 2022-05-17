<script setup lang="ts">
import { NTabs, NTabPane, NAlert, NEl } from 'naive-ui';
import ProcessSelector from '@/components/ProcessSelector.vue';
import FileSelector from '@/components/FileSelector.vue';
import { effect, reactive, ref } from 'vue';
import { Api } from '@/api';
import { usePlugin } from '@/hooks/usePlugin';
const debugPath = window.location.origin + '/debug';

usePlugin();
const debuggingProcess = reactive({
  length: 0,
  error: false,
});
effect(() => {
  setTimeout(async () => {
    const { ok, data } = await Api.DebuggingProcess.list();
    debuggingProcess.error = !ok;
    if (ok) {
      debuggingProcess.length = data.length;
    } else {
      debuggingProcess.length = 0;
    }
  }, 1000);
});
</script>

<template>
  <div class="page">
    <div style="display: flex; flex-direction: column; width: 60%">
      <n-alert type="error" v-if="debuggingProcess.error"> 无法请求到调试中进程，请检查网络设置。 </n-alert>
      <n-alert type="info" v-else-if="debuggingProcess.length !== 0">
        你还有进程正在调试中，如果想要继续调试，请打开 {{ debugPath }}
      </n-alert>
      <h1 class="title">BeDbg</h1>
      <n-el>
        <n-tabs default-value="create" type="line">
          <n-tab-pane name="create" tab="调试文件">
            <file-selector></file-selector>
          </n-tab-pane>
          <n-tab-pane name="attach" tab="附加到进程">
            <process-selector></process-selector>
          </n-tab-pane>
        </n-tabs>
      </n-el>
    </div>
  </div>
</template>

<style scoped>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}

.page {
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
}

.title {
  font-size: 3rem;
  text-align: center;
}
</style>
