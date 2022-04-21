import { DebuggerEvents, DebuggerEventTypes } from '@/dto/debuggerEvent';

type DebuggerEvent = typeof DebuggerEventTypes[number];
type DebuggerEventListener<EventType extends DebuggerEvent> = (...args: DebuggerEvents[EventType]) => void;

/**
 * DebuggerEventSource is a class that allows to subscribe to debugger events from server.
 */
export class DebuggerEventSource {
  #eventListeners: { [eventType in DebuggerEvent]: DebuggerEventListener<eventType>[] };
  #eventSource: EventSource;

  constructor(url: string) {
    this.#eventSource = new EventSource(url);

    this.#eventSource.onopen = () => {
      console.log('DebuggerEventSource connected');
    };
    this.#eventSource.onerror = (event: Event) => {
      console.log('DebuggerEventSource error:', event);
    };

    this.#eventListeners = {} as any; // We intentionally use any here to avoid type errors. Event listener lists are initialized below.
    DebuggerEventTypes.forEach(type => {
      this.#eventListeners[type] = [];
      this.#eventSource.addEventListener(type, event => {
        const payload = JSON.parse(event.data);
        this.#eventListeners[type].forEach(listener => listener(payload));
      });
    });
  }

  /**
   * Listen to a specific event.
   * @param type Event type.
   * @param listener Event listener.
   */
  addEventListener<EventType extends DebuggerEvent>(type: EventType, listener: DebuggerEventListener<EventType>) {
    this.#eventListeners[type].push(listener);
  }

  /**
   * Listen to a specific event once.
   * @param type Event type.
   * @param listener Event listener.
   */
  addEventListenerOnce<EventType extends DebuggerEvent>(type: EventType, listener: DebuggerEventListener<EventType>) {
    const onceListener = (...args: DebuggerEvents[EventType]) => {
      this.removeEventListener(type, onceListener);
      listener(...args);
    };
    this.addEventListener(type, onceListener);
  }

  /**
   * Remove an event listener.
   * @param type Event type.
   * @param listener Event listener.
   */
  removeEventListener<EventType extends DebuggerEvent>(type: EventType, listener: DebuggerEventListener<EventType>) {
    const index = this.#eventListeners[type].indexOf(listener);
    if (index >= 0) {
      this.#eventListeners[type].splice(index, 1);
    }
  }

  /**
   * Close the event source.
   */
  close() {
    this.#eventSource.close();
  }
}
