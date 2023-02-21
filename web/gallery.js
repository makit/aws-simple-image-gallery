API_GATEWAY_URL = 'https://3eru9j9le4.execute-api.eu-west-1.amazonaws.com/prod'

window.onload = () => {
  fetch(`{API_GATEWAY_URL}/categories`).then(r=>r.json()).then(data => 
  {
    console.log(data);
    //use data
   });
};