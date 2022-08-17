import Button from '../Button'
import Dropdown from '../Dropdown'
import Form from '../Form'
import './Form02.css'
import { useState } from 'react'

const Form02 = (props) => {

    const [nome, setNome] = useState('')
    const [cargo, setCargo] = useState('')
    const [imagem, setImagem] = useState('')
    const [time, setTime] = useState('') 

    const onSave = (event) => {
        event.preventDefault();
        props.aoColaboradorCadastrado({
            nome,
            cargo,
            imagem,
            time
        })
        setNome('')
        setCargo('')
        setImagem('')
        setTime('')
    }

    return (
        <section className="form02">
            <form onSubmit={onSave}>
                <h2>Preencha os dados para criar o card do colaborador</h2>
                <Form 
                obrigatorio={true} 
                label="Nome" 
                placeholder="Digite o seu nome"
                value={nome}
                onChange={value => setNome(value)} 
                />
                <Form 
                obrigatorio={true} 
                label="Cargo" 
                placeholder="Digite o seu cargo" 
                value={cargo}
                onChange={value => setCargo(value)} 
                />
                <Form 
                label="Imagem" 
                placeholder="Digite o endereço da imagem" 
                value={imagem}
                onChange={value => setImagem(value)} 
                />
                <Dropdown 
                obrigatorio={true}  
                label="Time" 
                itens={props.times}
                value={time}
                onChange={value => setTime(value)}
                />
                <Button text="Criar card"/>
            </form>
        </section>
    )
}

export default Form02