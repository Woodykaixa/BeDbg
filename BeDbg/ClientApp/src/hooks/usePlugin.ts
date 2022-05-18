import { BeDbg } from '@/plugin';
import { defineStore } from 'pinia';
import { Api } from '@/api';
import { effect, onUpdated, Ref, ref, computed } from 'vue';
import { useNotification } from 'naive-ui';
import { DebuggerEventSource } from '@/util/debuggerEventSource';
import { DebuggerEventTypes } from '@/dto/debuggerEvent';

type Plugin = BeDbg.Plugin;

type PluginMessage = {
  data: string;
  time: Date;
  event: typeof DebuggerEventTypes[number];
};

type PluginInstance = {
  output: PluginMessage[];
  error: PluginMessage[];
  plugin: Plugin;
};

type EventHandlerName<PluginKey extends keyof Plugin = keyof Plugin> = PluginKey extends `on${string}`
  ? PluginKey
  : never;

/**
 * Initialize a global object contains api and commands, thus users can call it from the console.
 * @param installer
 */
function initializeGlobalPluginEnvironment(installer: (plugin: Plugin) => void) {
  window.beDbg = {
    installPlugin: installer,
    plugin: {},
  };
}

export const usePlugin = defineStore('BeDbg/Plugin', () => {
  const plugins = ref<PluginInstance[]>([]);

  const notification = ref(useNotification());
  onUpdated(() => {
    notification.value = useNotification();
  });

  const installPluginImpl = (eventSource: DebuggerEventSource, instance: PluginInstance) => {
    console.log('plugin', instance.plugin.name, 'installing');

    DebuggerEventTypes.forEach(eventType => {
      const handlerName = `on${eventType[0].toUpperCase()}${eventType.slice(1)}` as EventHandlerName;
      const pluginHandler = instance.plugin[handlerName];
      if (pluginHandler) {
        const messagePrinter = (message: string, channel: 'output' | 'error') => {
          const pluginMessage: PluginMessage = {
            data: message,
            time: new Date(),
            event: eventType,
          };
          instance[channel].push(pluginMessage);
        };
        // @ts-ignore
        eventSource.addEventListener(eventType, payload => {
          pluginHandler(
            {
              api: Api,
              notification: notification.value,
              printOutput: output => messagePrinter(output, 'output'),
              printError: output => messagePrinter(output, 'error'),
            },
            // @ts-ignore
            payload
          );
        });
      }
    });
    window.beDbg.plugin[instance.plugin.name] = {
      output: [],
      error: [],
    };
    if (instance.plugin.commands) {
      window.beDbg.plugin[instance.plugin.name].commands = instance.plugin.commands;
    }

    notification.value.success({
      title: '插件安装成功',
      content: `插件 ${instance.plugin.name} 已成功安装，您可以在控制台查看插件的日志。`,
      duration: 2000,
    });
  };

  const debuggerEventSource = ref<DebuggerEventSource | null>(null);
  const pluginInstalled = (plugin: Plugin) => plugins.value.some(p => p.plugin.name === plugin.name);

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
    const instance: PluginInstance = {
      plugin,
      output: [],
      error: [],
    };
    plugins.value.push(instance);
    if (!!debuggerEventSource.value) {
      installPluginImpl(debuggerEventSource.value as any, instance);
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
    initializeGlobalPluginEnvironment(installPlugin);
  });

  return {
    plugins,
    installPlugin,
    setEventSource,
    removeEventSource,
  };
});
