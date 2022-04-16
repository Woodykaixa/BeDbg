import { DebuggerEventPayload, DebuggerEvents, DebuggerEventTypes } from '@/dto/debuggerEvent';

type DebuggerEvent = typeof DebuggerEventTypes[number];
type DebuggerEventListener<PayloadType extends DebuggerEventPayload> = (payload: PayloadType) => void;

export class DebuggerEventSource {
  private eventListeners: { [eventType in DebuggerEvent]: DebuggerEventListener<DebuggerEvents[eventType]>[] };
  private eventSource: EventSource;
  constructor(url: string) {
    this.eventListeners = {
      exception: [],
      createThread: [],
      createProcess: [],
      exitThread: [],
      exitProcess: [],
      loadDll: [],
      unloadDll: [],
      outputDebugString: [],
      rip: [],
    };

    this.eventSource = new EventSource(url);

    this.eventSource.onopen = () => {
      console.log('DebuggerEventSource connected');
    };
    this.eventSource.onerror = (event: Event) => {
      console.log('DebuggerEventSource error:', event);
    };

    DebuggerEventTypes.forEach(type => {
      this.eventSource.addEventListener(type, event => {
        const payload = JSON.parse(event.data);
        this.eventListeners[type].forEach(listener => listener(payload));
      });
    });
  }
  addEventListener<EventType extends DebuggerEvent>(
    type: EventType,
    listener: DebuggerEventListener<DebuggerEvents[EventType]>
  ) {
    this.eventListeners[type].push(listener);
  }
  removeEventListener<EventType extends DebuggerEvent>(
    type: EventType,
    listener: DebuggerEventListener<DebuggerEvents[EventType]>
  ) {
    const index = this.eventListeners[type].indexOf(listener);
    if (index >= 0) {
      this.eventListeners[type].splice(index, 1);
    }
  }
}
