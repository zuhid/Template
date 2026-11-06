<!-------------------------------------------------- script -------------------------------------------------->
<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { numericTypeStore } from '../store';

const props = defineProps({ id: { type: String, default: null } });

const router = useRouter();
const form = ref({ smallintType: '', integerType: '', bigintType: '' });
const errors = ref({});

const LONG_MIN = -9223372036854775808n;
const fields = [
  { key: 'smallintType', label: 'Smallint', min: 0n, max: 100n },
  { key: 'integerType', label: 'Integer', min: 0n, max: 2147483647n },
  { key: 'bigintType', label: 'Bigint', min: LONG_MIN, max: 500n },
];

onMounted(async () => {
  if (!props.id) return;
  const row = await numericTypeStore.get(props.id);
  if (row) form.value = { smallintType: String(row.smallintType), integerType: String(row.integerType), bigintType: String(row.bigintType) };
  else router.replace('/numeric-types');
});

function back() {
  router.push('/numeric-types');
}

function validate() {
  const result = {};
  for (const f of fields) {
    const text = String(form.value[f.key]).trim();
    if (text === '') result[f.key] = `${f.label} is required`;
    else if (!/^-?\d+$/.test(text)) result[f.key] = `${f.label} must be a whole number`;
    else {
      const n = BigInt(text);
      if (n < f.min || n > f.max) result[f.key] = `${f.label} must be between ${f.min} and ${f.max}`;
    }
  }
  errors.value = result;
  return Object.keys(result).length === 0;
}

async function save() {
  if (!validate()) return;
  const model = {
    smallintType: Number(form.value.smallintType),
    integerType: Number(form.value.integerType),
    bigintType: BigInt(form.value.bigintType.trim()),
  };
  const result = props.id
    ? await numericTypeStore.update({ id: props.id, ...model })
    : await numericTypeStore.create(model);
  if (result !== undefined) {
    await numericTypeStore.forceRefresh();
    back();
  }
}
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <Card :id="id ? 'edit_numeric_type_form' : 'add_numeric_type_form'" :title="id ? 'Edit Numeric Type' : 'Add Numeric Type'" saveText="Save" cancelText="Cancel" @onSave="save" @onCancel="back">
    <template v-for="f in fields" :key="f.key">
      <Text :label="f.label" v-model="form[f.key]" />
      <small v-if="errors[f.key]" class="field-error" :data-testid="f.key + '_error'">{{ errors[f.key] }}</small>
    </template>
  </Card>
</template>

<!-------------------------------------------------- style -------------------------------------------------->
<style scoped>
#edit_numeric_type_form,
#add_numeric_type_form {
  width: 400px;
}

.field-error {
  display: block;
  color: var(--danger-text);
  background-color: var(--danger-bg);
}
</style>
