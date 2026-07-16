import {
    Card,
    Col,
    Row,
    Statistic,
    Table,
    Tag
} from "antd";

import {
    CheckCircleOutlined,
    FolderOpenOutlined,
    BugOutlined,
    PlayCircleOutlined
} from "@ant-design/icons";

import { useEffect, useState } from "react";

import axios from "axios";

export default function Dashboard() {

    const [data,setData]=useState<any>();

    useEffect(()=>{

        axios
        .get("http://localhost:5072/api/dashboard")
        .then(r=>setData(r.data));

    },[]);

    if(!data)
        return null;

    return(

<div style={{padding:20}}>

<div
        style={{
          display: "flex",
          justifyContent: "space-between",
          marginBottom: 16,
        }}
      >
        <h2>Dashboard</h2>
      </div>

<Row gutter={16}>

<Col span={6}>
<Card>
<Statistic
title="Test Cases"
value={data.totalTests}
prefix={<BugOutlined/>}
/>
</Card>
</Col>

<Col span={6}>
<Card>
<Statistic
title="Suites"
value={data.totalSuites}
prefix={<FolderOpenOutlined/>}
/>
</Card>
</Col>

<Col span={6}>
<Card>
<Statistic
title="Executions"
value={data.totalExecutions}
prefix={<PlayCircleOutlined/>}
/>
</Card>
</Col>

<Col span={6}>
<Card>
<Statistic
title="Pass Rate"
value={data.passRate}
suffix="%"
prefix={<CheckCircleOutlined/>}
/>
</Card>
</Col>

</Row>

<br/>

<Row gutter={16}>

<Col span={24}>

<Card title="Recent Executions">

<Table

pagination={false}

dataSource={data.recentExecutions}

rowKey="runId"

>

<Table.Column

title="Suite"

dataIndex="suite"

/>

<Table.Column

title="Passed"

render={(_,r:any)=>

<Tag color="green">

{r.passed}

</Tag>

}

/>

<Table.Column

title="Failed"

render={(_,r:any)=>

<Tag color="red">

{r.failed}

</Tag>

}

/>

</Table>

</Card>

</Col>

{/* <Col span={10}>

<Card title="Top Failed Tests">

<Table

pagination={false}

dataSource={data.topFailures}

rowKey="test"

>

<Table.Column

title="Test"

dataIndex="test"

/>

<Table.Column

title="Failures"

dataIndex="count"

/>

</Table>

</Card>

</Col> */}

</Row>

</div>

);

}