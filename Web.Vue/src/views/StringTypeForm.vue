<!-------------------------------------------------- script -------------------------------------------------->
<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { stringTypeStore } from '../store';

const props = defineProps({ id: { type: String, default: null } });

const router = useRouter();
const form = ref({ charType: '', varcharType: '', textType: '' });

onMounted(async () => {
  if (!props.id) return;
  const row = await stringTypeStore.get(props.id);
  if (row) form.value = { charType: row.charType, varcharType: row.varcharType, textType: row.textType };
  else router.replace('/string-types');
});

function back() {
  router.push('/string-types');
}

async function save() {
  const result = props.id
    ? await stringTypeStore.update({ id: props.id, ...form.value })
    : await stringTypeStore.create(form.value);
  if (result !== undefined) {
    await stringTypeStore.forceRefresh();
    back();
  }
}
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <Card :id="id ? 'edit_string_type_form' : 'add_string_type_form'" :title="id ? 'Edit String Type' : 'Add String Type'" saveText="Save" cancelText="Cancel" @onSave="save" @onCancel="back">
    <Text label="Char" v-model="form.charType" />
    <Text label="Varchar" v-model="form.varcharType" />
    <Text label="Text" v-model="form.textType" />
  </Card>
</template>

<!-------------------------------------------------- style -------------------------------------------------->
<style scoped>
#edit_string_type_form,
#add_string_type_form {
  width: 400px;
}
</style>
