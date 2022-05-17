import { DebuggerEventTypes, DebuggerEvents } from '@/dto/debuggerEvent';
import { Api } from '@/api';
import { NotificationApi } from 'naive-ui';

export namespace BeDbg {
  type DebuggerEvent = typeof DebuggerEventTypes[number];
  type FormatEventHandlerName<T> = T extends `${infer First}${infer Rest}` ? `on${Uppercase<First>}${Rest}` : never;

  type DebuggerEventHandlerNames = FormatEventHandlerName<DebuggerEvent>;
  type HandlerNameToOrigin<Name extends DebuggerEventHandlerNames> = Name extends `on${infer First}${infer Rest}`
    ? `${Lowercase<First>}${Rest}`
    : never;

  export type PluginEventContext = {
    api: typeof Api;
    notification: NotificationApi;
  };

  type PluginEventHandler<EventType extends DebuggerEvent> = (
    context: PluginEventContext,
    ...args: [payload: DebuggerEvents[EventType][0]]
  ) => void;

  type PluginEvents = {
    /**
     * When debugger trigger this event, your code will be called
     */
    [K in DebuggerEventHandlerNames]?: PluginEventHandler<HandlerNameToOrigin<K>>;
  };

  export interface Plugin extends PluginEvents {
    /** Name of your plugin is used as unique identifier. You should guarantee it is unique, or your plugin is installed before another plugin which has the same name. */
    name: string;
    /** Your name. */
    author: string;
    /** Version of your plugin. It might be used by BeDbg in the future. */
    version: string;
    /** Description of your plugin. Introduce your plugin here. */
    description: string;
  }
}
