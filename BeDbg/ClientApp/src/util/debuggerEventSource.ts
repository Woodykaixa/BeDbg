import { DebuggerEvents, DebuggerEventTypes } from '@/dto/debuggerEvent';

type DebuggerEvent = typeof DebuggerEventTypes[number];
type DebuggerEventListener<EventType extends DebuggerEvent> = (...args: DebuggerEvents[EventType]) => void;

export class DebuggerEventSource {
  #eventListeners: { [eventType in DebuggerEvent]: DebuggerEventListener<eventType>[] };
  #eventSource: EventSource;
  constructor(url: string) {
    this.#eventListeners = {
      exception: [],
      createThread: [],
      createProcess: [],
      exitThread: [],
      exitProcess: [],
      loadDll: [],
      unloadDll: [],
      outputDebugString: [],
      rip: [],
      exitProgram: [],
    };

    this.#eventSource = new EventSource(url);

    this.#eventSource.onopen = () => {
      console.log('DebuggerEventSource connected');
    };
    this.#eventSource.onerror = (event: Event) => {
      console.log('DebuggerEventSource error:', event);
    };

    DebuggerEventTypes.forEach(type => {
      this.#eventSource.addEventListener(type, event => {
        const payload = JSON.parse(event.data);
        this.#eventListeners[type].forEach(listener => listener(payload));
      });
    });
  }

  addEventListener<EventType extends DebuggerEvent>(type: EventType, listener: DebuggerEventListener<EventType>) {
    this.#eventListeners[type].push(listener);
  }

  removeEventListener<EventType extends DebuggerEvent>(type: EventType, listener: DebuggerEventListener<EventType>) {
    const index = this.#eventListeners[type].indexOf(listener);
    if (index >= 0) {
      this.#eventListeners[type].splice(index, 1);
    }
  }
}
