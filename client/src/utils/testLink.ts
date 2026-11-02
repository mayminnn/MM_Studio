// TestLink deep link; the project prefix is the part of the ID before the "-" (e.g. "tFJ" in "tFJ-19")
const TESTLINK_URL = "http://swtest4-dt:81/testlink/linkto.php";

export const testLinkUrl = (id: string) => {
  const prefix = id.substring(0, id.lastIndexOf("-")) || "tFJ";
  return `${TESTLINK_URL}?tprojectPrefix=${encodeURIComponent(prefix)}&item=testcase&id=${encodeURIComponent(id)}`;
};
