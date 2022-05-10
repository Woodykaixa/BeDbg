<script lang="ts" setup>
import { NElement, NButton, NIcon } from 'naive-ui';
import {
  MoreOutlined,
  ArrowDownOutlined,
  ArrowUpOutlined,
  ArrowRightOutlined,
  CloseOutlined,
  PlaySquareOutlined,
} from '@vicons/antd';

import { useDebugData } from '@/hooks/useDebugData';
import { useRouter } from 'vue-router';
import { useNotification } from 'naive-ui';
import { Api } from '@/api';
import { useLoadingStates } from '@/hooks/useLoadingStates';

const debugData = useDebugData();
const router = useRouter();
const notification = useNotification();
const loadingStates = useLoadingStates();

const stopDebug = async () => {
  const { ok, data } = await Api.DebuggingProcess.detachProcess(debugData.mainProcess.id);
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

function apiCall<ApiType extends (...args: any[]) => any, ArgTypes extends Parameters<ApiType>, Return extends ReturnType<ApiType>>(
  api: ApiType,
  ...args: ArgTypes[]
): Return {
  const backup = loadingStates.backupLoadingStates();
  loadingStates.setAllLoading();
  const result = api(...args);
  loadingStates.restoreLoadingStates(backup);
  return result;
}

</script>

<template>
  <n-element>
    <div justify="center" class="debug-control" align="center">
      <n-button
        quaternary
        type="success"
        title="继续运行"
        :disabled="loadingStates.panelState !== 'ready'"
        @click="apiCall(Api.DebuggingProcess.continue, debugData.mainProcess.id, debugData.mainProcess.mainThread.id)"
      >
        <template #icon>
          <n-icon size="20">
            <play-square-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button
        quaternary
        type="success"
        title="单步执行 (Step In)"
        :disabled="loadingStates.panelState !== 'ready'"
        @click="apiCall(Api.DebuggingProcess.stepIn, debugData.mainProcess.id, debugData.mainProcess.mainThread.id)"
      >
        <template #icon>
          <n-icon size="20">
            <arrow-down-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary type="success" title="单步执行 (Step Over)" :disabled="loadingStates.panelState !== 'ready'">
        <template #icon>
          <n-icon size="20">
            <arrow-right-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary type="success" title="退出函数 (Step Out)" :disabled="loadingStates.panelState !== 'ready'">
        <template #icon>
          <n-icon size="20">
            <arrow-up-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary type="error" title="结束调试" @click="stopDebug">
        <template #icon>
          <n-icon size="20">
            <close-outlined />
          </n-icon>
        </template>
      </n-button>
      <n-button quaternary title="其他功能">
        <template #icon>
          <n-icon size="20">
            <more-outlined />
          </n-icon>
        </template>
      </n-button>
    </div>
  </n-element>
</template>

<style scoped>
.debug-control {
  background: var(--modal-color);
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 8px;
}
</style>
