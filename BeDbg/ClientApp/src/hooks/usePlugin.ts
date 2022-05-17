import { BeDbg } from '@/plugin';
import { defineStore } from 'pinia';
import { Api } from '@/api';
import { effect, onUpdated, ref } from 'vue';
import { useNotification } from 'naive-ui';
import { DebuggerEventSource } from '@/util/debuggerEventSource';
import { DebuggerEventTypes } from '@/dto/debuggerEvent';

type Plugin = BeDbg.Plugin;
type PluginEventContext = BeDbg.PluginEventContext;

type EventHandlerName<PluginKey extends keyof Plugin = keyof Plugin> = PluginKey extends `on${string}`
  ? PluginKey
  : never;

export const usePlugin = defineStore('BeDbg/Plugin', () => {
  const plugins = ref([] as Plugin[]);
  const notification = ref(useNotification());
  onUpdated(() => {
    notification.value = useNotification();
  });
  const installPluginImpl = (eventSource: DebuggerEventSource, plugin: Plugin) => {
    console.log('plugin', plugin.name, 'installing');

    DebuggerEventTypes.forEach(eventType => {
      const handlerName = `on${eventType[0].toUpperCase()}${eventType.slice(1)}` as EventHandlerName;
      const pluginHandler = plugin[handlerName];
      if (pluginHandler) {
        // @ts-ignore
        eventSource.addEventListener(eventType, payload => {
          pluginHandler(
            {
              api: Api,
              notification: notification.value,
            },
            // @ts-ignore
            payload
          );
        });
      }
    });

    notification.value.success({
      title: '插件安装成功',
      content: `插件 ${plugin.name} 已成功安装，您可以在控制台查看插件的日志。`,
      duration: 2000,
    });
  };

  const debuggerEventSource = ref<DebuggerEventSource | null>(null);
  const pluginInstalled = (plugin: Plugin) => plugins.value.some(p => p.name === plugin.name);

  const installPlugin = (plugin: Plugin) => {
    if (pluginInstalled(plugin)) {
      notification.value.info({
        title: '插件已安装',
        content: `插件 ${plugin.name} 已安装`,
        description: 'BeDbg 以插件名作为唯一标识符，且根据插件添加顺序安装插件。请确保插件名唯一。',
        duration: 2000,
      });
      return;
    }
    plugins.value.push(plugin);
    if (!!debuggerEventSource.value) {
      installPluginImpl(debuggerEventSource.value as any, plugin);
    }
  };

  const setEventSource = (eventSource: DebuggerEventSource) => {
    debuggerEventSource.value = eventSource;
    plugins.value.forEach(plugin => {
      installPluginImpl(eventSource, plugin);
    });
  };

  const removeEventSource = () => {
    debuggerEventSource.value = null;
  };

  effect(() => {
    // Set installer on global object, thus users can call it from the console.
    window['beDbg'] = {
      installPlugin,
    };
  });

  return {
    plugins,
    installPlugin,
    setEventSource,
    removeEventSource,
  };
});
