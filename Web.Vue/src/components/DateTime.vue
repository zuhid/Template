<script setup>
import { computed } from "vue";

/************************************************** Props **************************************************/
const model = defineModel({ type: String, default: "" });
const props = defineProps({
  id: { type: String, default: "" },
  label: { type: String, default: "" },
  type: { type: String, default: "datetime-local" }, // date | time | datetime-local
  disabled: { type: Boolean, default: false },
});

/************************************************** Emits **************************************************/
defineEmits(["blur"]);

/************************************************** Computed **************************************************/
const computedId = computed(() => props.id || props.label.toLowerCase().replace(/\s+/g, "_"));
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <div class="field datetime">
    <label v-if="label" :for="computedId">{{ label }}</label>
    <input :id="computedId" :type="type" step="1" :disabled="disabled" v-model="model" @blur="$emit('blur', $event)" />
  </div>
</template>
