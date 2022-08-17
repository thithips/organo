import './Button.css'

const Button = (props) => {
    return (<button className='button'>
        {props.text}
    </button>)

}

export default Button