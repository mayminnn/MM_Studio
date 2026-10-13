import { Form, Input, Modal } from "antd";

export type RunConfig = {
  projectName: string;
  version: string;
  installerVersion: string;
};

const STORAGE_KEY = "mmStudio.runConfig";

const DEFAULT_CONFIG: RunConfig = { projectName: "OX01G10", version: "v1", installerVersion: "1.0" };

// Last-used values, so the dialog is pre-filled on the next run
const loadLastConfig = (): RunConfig => {
  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? "null");

    // Defaults fill any field missing from an older saved config
    return { ...DEFAULT_CONFIG, ...saved };
  } catch {
    return DEFAULT_CONFIG;
  }
};

const saveLastConfig = (config: RunConfig) => {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(config));
  } catch {
    // Storage unavailable; the dialog just won't remember the values
  }
};

type Props = {
  open: boolean;
  testCount: number;
  onCancel: () => void;
  onRun: (config: RunConfig) => void;
};

export default function RunConfigModal({ open, testCount, onCancel, onRun }: Props) {
  const [form] = Form.useForm<RunConfig>();

  const handleOk = async () => {
    let values: RunConfig;

    try {
      values = await form.validateFields();
    } catch {
      return; // Field errors are shown inline
    }

    const config = {
      projectName: values.projectName.trim(),
      version: values.version.trim(),
      installerVersion: values.installerVersion.trim(),
    };

    saveLastConfig(config);
    onRun(config);
  };

  return (
    <Modal
      title="Run Tests"
      open={open}
      onCancel={onCancel}
      onOk={handleOk}
      okText={`▶ Run ${testCount} test(s)`}
      destroyOnHidden
    >
      {/* Remounted on every open (destroyOnHidden), so this re-reads the last-used values */}
      <Form form={form} layout="vertical" preserve={false} initialValues={loadLastConfig()}>
        <Form.Item
          label="Project Name"
          name="projectName"
          rules={[{ required: true, whitespace: true, message: "Enter the project name" }]}
        >
          <Input placeholder="e.g. OX01G10" />
        </Form.Item>

        <Form.Item
          label="Version"
          name="version"
          rules={[{ required: true, whitespace: true, message: "Enter the version" }]}
        >
          <Input placeholder="e.g. v1" />
        </Form.Item>

        <Form.Item
          label="Installer Version"
          name="installerVersion"
          rules={[{ required: true, whitespace: true, message: "Enter the installer version" }]}
        >
          <Input placeholder="e.g. 1.0" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
