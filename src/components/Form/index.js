import './Form.css'
import { useState } from 'react'

const Form = (props) => {

    const placeholderModificada = `${props.placeholder}...`


    const onDigit = (event) => {
        props.onChange(event.target.value)
    }
    
    return (
        <div className="form">
            <label>
                {props.label}
            </label>
            <input value={props.value} onChange={onDigit} required={props.obrigatorio} placeholder={placeholderModificada}/>
        </div>
    )
}

export default Form