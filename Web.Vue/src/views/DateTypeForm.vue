<!-------------------------------------------------- script -------------------------------------------------->
<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { dateTypeStore } from '../store';

const props = defineProps({ id: { type: String, default: null } });

const router = useRouter();
const form = ref({ dateTimeWithoutTzType: '', dateTimeType: '', timeSpanType: '', dateOnlyType: '', timeOnlyType: '' });

const pad = (n) => String(n).padStart(2, '0');

// API value -> input value
const toTime = (v) => (v ? v.slice(0, 8) : '');
const toDate = (v) => (v ? v.slice(0, 10) : '');
const toLocalNoTz = (v) => (v ? v.slice(0, 19) : '');
const toLocalFromInstant = (v) => {
  if (!v) return '';
  const d = new Date(/(Z|[+-]\d\d:?\d\d)$/.test(v) ? v : v + 'Z');
  if (isNaN(d)) return '';
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
};

// input value -> API value (empty becomes null)
const fullTime = (v) => (v ? (v.length === 5 ? v + ':00' : v) : null);
const fullLocal = (v) => (v ? (v.length === 16 ? v + ':00' : v) : null);
const instant = (v) => (v ? new Date(v).toISOString() : null);

function toForm(row) {
  return {
    dateTimeWithoutTzType: toLocalNoTz(row.dateTimeWithoutTzType),
    dateTimeType: toLocalFromInstant(row.dateTimeType),
    timeSpanType: toTime(row.timeSpanType),
    dateOnlyType: toDate(row.dateOnlyType),
    timeOnlyType: toTime(row.timeOnlyType),
  };
}

function toModel() {
  return {
    dateTimeWithoutTzType: fullLocal(form.value.dateTimeWithoutTzType),
    dateTimeType: instant(form.value.dateTimeType),
    timeSpanType: fullTime(form.value.timeSpanType),
    dateOnlyType: form.value.dateOnlyType || null,
    timeOnlyType: fullTime(form.value.timeOnlyType),
  };
}

onMounted(async () => {
  if (!props.id) return;
  const row = await dateTypeStore.get(props.id);
  if (row) form.value = toForm(row);
  else router.replace('/date-types');
});

function back() {
  router.push('/date-types');
}

async function save() {
  const model = toModel();
  const result = props.id
    ? await dateTypeStore.update({ id: props.id, ...model })
    : await dateTypeStore.create(model);
  if (result !== undefined) {
    await dateTypeStore.forceRefresh();
    back();
  }
}
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <Card :id="id ? 'edit_date_type_form' : 'add_date_type_form'" :title="id ? 'Edit Date Type' : 'Add Date Type'" saveText="Save" cancelText="Cancel" @onSave="save" @onCancel="back">
    <DateTime label="DateTime (no TZ)" id="date_time_without_tz_type" type="datetime-local" v-model="form.dateTimeWithoutTzType" />
    <DateTime label="DateTime" id="date_time_type" type="datetime-local" v-model="form.dateTimeType" />
    <DateTime label="TimeSpan" id="time_span_type" type="time" v-model="form.timeSpanType" />
    <DateTime label="Date" id="date_only_type" type="date" v-model="form.dateOnlyType" />
    <DateTime label="Time" id="time_only_type" type="time" v-model="form.timeOnlyType" />
  </Card>
</template>

<!-------------------------------------------------- style -------------------------------------------------->
<style scoped>
#edit_date_type_form,
#add_date_type_form {
  width: 400px;
}
</style>
