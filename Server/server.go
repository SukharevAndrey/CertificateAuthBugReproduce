package main

import (
	"crypto/tls"
	"log"
	"io/ioutil"
	"crypto/x509"
	"net/http"
)

func main() {
	caCert, err := ioutil.ReadFile("ca.cer")
        if err != nil {
		log.Fatal(err)
	}
	caCertPool := x509.NewCertPool()
	caCertPool.AppendCertsFromPEM(caCert)
	cfg := &tls.Config{
		ClientAuth: tls.RequireAndVerifyClientCert,
                InsecureSkipVerify: true,
                //ClientCAs: caCertPool,
	}
	srv := &http.Server{
		Addr:      ":8443",
		Handler:   &handler{},
		TLSConfig: cfg,
	}
    log.Fatal(srv.ListenAndServeTLS("certificate.cer", "private.key"))
}

type handler struct{}

func (h *handler) ServeHTTP(w http.ResponseWriter, req *http.Request) {
	w.Write([]byte("PONG"))
}
