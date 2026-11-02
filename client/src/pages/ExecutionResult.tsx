import { useEffect, useState } from "react";
import { Card, Tag } from "antd";
import axios from "axios";
import { useParams } from "react-router-dom";
import { statusColor } from "../utils/resultStatus";
import BackButton from "../components/BackButton";
import { getTestLinkIdMap, testLinkIdsFor } from "../services/testcaseService";
import { testLinkUrl } from "../utils/testLink";

export default function ExecutionResult(){

    const { id } = useParams();

    const [result,setResult]=useState<any>();

    const [testLinkIds,setTestLinkIds]=useState<Record<string, string[]>>({});

    useEffect(()=>{

        axios
            .get(`http://localhost:5072/api/results/${id}`)
            .then(res=>setResult(res.data));

    },[id]);

    useEffect(()=>{

        getTestLinkIdMap()
            .then(setTestLinkIds)
            .catch(()=>setTestLinkIds({}));

    },[]);

    if(!result)
        return null;

    const ids = testLinkIdsFor(testLinkIds, result.className);

    return(

        <div style={{padding:20}}>

            <BackButton to={`/executions/${result.runId}`} label="Back" />

            <Card title={result.displayName}>

                <p>

                    <b>Test ID : </b>

                    {result.className || "-"}

                </p>

                <p>

                    <b>TestLink ID : </b>

                    {ids.length === 0
                        ? "-"
                        : ids.map(id =>
                            <Tag key={id}>
                                <a href={testLinkUrl(id)} target="_blank" rel="noopener noreferrer">
                                    {id}
                                </a>
                            </Tag>
                        )}

                </p>

                <p>

                    <b>Status :</b>

                    <Tag color={statusColor(result.status)}>
                        {result.status}
                    </Tag>

                </p>

                <p>

                    <b>Executed : </b>

                    {result.status==="Not Run"
                        ? "-"
                        : new Date(result.executedAt).toLocaleString()}

                </p>

                <h3>Output</h3>

                <pre style={{overflowX:"auto"}}>{result.output}</pre>

                <h3>Error</h3>

                <pre style={{overflowX:"auto"}}>{result.error}</pre>

            </Card>

        </div>

    );

}