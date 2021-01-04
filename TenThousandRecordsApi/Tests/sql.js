import { check } from 'k6';
import http from 'k6/http';

export let options = {
    vus: 50,
    duration: '30s'
};

export default function() {
    let res = http.get('https://localhost:5001/api/sql');
    check(res, {
        'is status 200' : (r) => r.status === 200
    });
}