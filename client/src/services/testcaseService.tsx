import axios from "axios";

const API_URL = "http://localhost:5072/api/testcases";

export const getTestCases = async () => {
  const res = await axios.get(API_URL);
  return res.data;
};

// Results only carry the class name; TestLink IDs come from the test case library
export const getTestLinkIdMap = async (): Promise<Record<string, string[]>> => {
  const cases: { className: string; testLinkIds?: string[] }[] = await getTestCases();
  return Object.fromEntries(cases.map(c => [c.className.toLowerCase(), c.testLinkIds ?? []]));
};

export const testLinkIdsFor = (map: Record<string, string[]>, className?: string) =>
  map[(className ?? "").toLowerCase()] ?? [];
