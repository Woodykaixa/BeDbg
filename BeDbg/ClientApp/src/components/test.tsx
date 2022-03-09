import { defineComponent, ref } from 'vue';

export const Test = defineComponent({
  setup() {
    const counter = ref(0);
    return {
      counter,
    };
  },
  render() {
    return (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <div>count: {this.counter}</div>
        <button
          onClick={e => {
            this.counter++;
          }}
        >
          click me
        </button>
      </div>
    );
  },
});
